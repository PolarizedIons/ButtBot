using System;
using System.Linq;
using System.Threading.Tasks;
using ButtBot.Core.Database;
using ButtBot.Discord.Exceptions;
using ButtBot.Library.Extentions;
using ButtBot.Library.Models;
using ButtBot.Library.Models.Database;
using Microsoft.EntityFrameworkCore;
using RMQCommandService.Models;
using Serilog;

namespace ButtBot.Core.Services
{
    public class ButtcoinService : IScopedDiService
    {
        private readonly DatabaseContext _db;

        public ButtcoinService(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<ButtcoinAccount> GetOrCreateAccount(string userId)
        {
            var existing = await _db.Accounts
                .AsQueryable()
                .Include(x => x.Stats)
                .FirstOrDefaultAsync(x => x.DeletedAt == null && x.DiscordUserId == userId);

            if (existing != null)
            {
                return existing;
            }

            Log.Debug("Creating new account for {UserId}", userId);
            var account = new ButtcoinAccount
            {
                DiscordUserId = userId,
                Stats = new ButtcoinStats(),
            };

            await _db.Accounts.AddAsync(account);
            await _db.SaveChangesAsync();

            return account;
        }

        public async Task<HolderAccount> GetOrCreateHolderAccount(string userId, BotPlatform platform)
        {
            if (platform == BotPlatform.Discord)
            {
                throw new ArgumentException($"Cannot create a holder account for platform {platform.GetValueName()}", nameof(platform));
            }

            var existing = await _db.HolderAccounts
                .FirstOrDefaultAsync(x =>
                    x.DeletedAt == null &&
                    x.UserId == userId &&
                    x.Platform == platform
                );

            if (existing != null)
            {
                return existing;
            }

            Log.Debug("Creating new holder account for {UserId} on {Platform}", userId, platform.GetValueName());
            var account = new HolderAccount
            {
                UserId = userId,
                Platform = platform,
            };

            await _db.HolderAccounts.AddAsync(account);
            await _db.SaveChangesAsync();

            return account;
        }

        public async Task MineCoin(string userId, BotPlatform platform, bool isBruteForce)
        {
            var linkedDiscordAccount = await GetLinkedAccount(userId, platform);

            if (platform == BotPlatform.Discord || linkedDiscordAccount != null)
            {
                var discordId = linkedDiscordAccount != null ? linkedDiscordAccount.DiscordUserId : userId;
                Log.Debug("{UserId} ({Platform}) mined a buttcoin (bruteforce? {IsBruteForce}) in their buttcoin account ({DiscordId})", userId, platform.GetValueName(), isBruteForce, discordId);

                var account = await GetOrCreateAccount(discordId);

                account.Balance += 1;
                account.Stats.AmountBruteforced += isBruteForce ? 1ul : 0ul;
                account.Stats.AmountMined += 1;
            }
            else
            {
                Log.Debug("{UserId} ({Platform}) mined a buttcoin (bruteforce? {IsBruteForce}) in their holder account", userId, platform.GetValueName(), isBruteForce);

                var account = await GetOrCreateHolderAccount(userId, platform);

                account.AmountMined += 1;
                account.AmountBruteforced += isBruteForce ? 1ul : 0ul;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<ButtcoinAccount> ActivateAccount(string userId)
        {
            var account = await GetOrCreateAccount(userId);
            if (!account.IsActive)
            {
                Log.Debug("Activating {UserId}'s account", account.DiscordUserId);
                account.IsActive = true;
                await _db.SaveChangesAsync();
            }

            return account;
        }

        public async Task<(ButtcoinAccount, ButtcoinAccount)> Transfer(string fromUserId, string toUserId, ulong amount, string reason)
        {
            var fromAccount = await GetOrCreateAccount(fromUserId);
            var toAccount = await GetOrCreateAccount(toUserId);

            if (fromAccount.DiscordUserId == toAccount.DiscordUserId)
            {
                Log.Debug("Failed to transfer {Amount} from {FromUserId} to {ToUserId} with reason '{Reason}', because they are the same person", amount, fromAccount.DiscordUserId, toAccount.DiscordUserId, reason);
                throw new SamePersonException();
            }

            if (amount > fromAccount.Balance)
            {
                Log.Debug("Failed to transfer {Amount} from {FromUserId} to {ToUserId} with reason '{Reason}', because of insufficient funds", amount, fromAccount.DiscordUserId, toAccount.DiscordUserId, reason);
                throw new NotEnoughFundsException();
            }

            if (!toAccount.IsActive)
            {
                Log.Debug("Failed to transfer {Amount} from {FromUserId} to {ToUserId} with reason '{Reason}', because the target doesn't have an active account", amount, fromAccount.DiscordUserId, toAccount.DiscordUserId, reason);
                throw new AccountNotActive();
            }

            Log.Debug("Transferred {Amount} from {FromUserId} to {ToUserId} with reason '{Reason}'", amount, fromAccount.DiscordUserId, toAccount.DiscordUserId, reason);

            fromAccount.Balance -= amount;
            toAccount.Balance += amount;

            fromAccount.Stats.AmountGifted += amount;
            toAccount.Stats.AmountReceived += amount;

            await _db.SaveChangesAsync();

            return (fromAccount, toAccount);
        }

        public async Task<EmptyResponse> LinkAccounts(string discordId, BotPlatform platform, string platformId)
        {
            if (await HolderAccountExists(platformId, platform))
            {
                Log.Debug("Transferring buttcoins from {PlatformId} ({Platform}) to buttcoin account {AccountId}", platformId, platform.GetValueName(), discordId);
                var account = await GetOrCreateAccount(discordId);
                var holderAccount = await GetOrCreateHolderAccount(platformId, platform);

                account.Balance += holderAccount.AmountMined;
                account.Stats.AmountBruteforced += holderAccount.AmountBruteforced;

                holderAccount.MarkDeleted();
            }

            await _db.LinkedAccounts.AddAsync(new LinkedAccounts
            {
                DiscordId = discordId,
                Platform = platform,
                PlatformId = platformId,
            });
            
            await _db.SaveChangesAsync();

            Log.Debug("Linked {PlatformId} ({Platform}) with buttcoin account {AccountId}", platformId, platform.GetValueName(), discordId);

            return new EmptyResponse();
        }

        private async Task<bool> HolderAccountExists(string userId, BotPlatform platform)
        {
            if (platform == BotPlatform.Discord)
            {
                return false;
            }

            return await _db.HolderAccounts
                .AnyAsync(x =>
                    x.DeletedAt == null &&
                    x.UserId == userId &&
                    x.Platform == platform
                );
        }

        private async Task<ButtcoinAccount?> GetLinkedAccount(string userId, BotPlatform platform)
        {
            return await _db.LinkedAccounts
                .Where(x => x.DeletedAt == null && x.PlatformId == userId && x.Platform == platform)
                .Join(
                    _db.Accounts,
                    linkedAccount => linkedAccount.DiscordId,
                    buttcoinAccount => buttcoinAccount.DiscordUserId,
                    (linkedAccount, buttcoinAccount) => buttcoinAccount
                )
                .FirstOrDefaultAsync();
        }
    }
}
