using System.Threading.Tasks;
using ButtBot.Database;
using ButtBot.Database.Models;
using ButtBot.Exceptions;
using ButtBot.Extentions;
using Discord;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ButtBot.Services
{
    public class ButtcoinService : IScopedDiService
    {
        private readonly DatabaseContext _db;

        public ButtcoinService(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<ButtcoinAccount> GetOrCreateAccount(IUser user)
        {
            return await GetOrCreateAccount(user.Id.ToString());
        }

        public async Task<ButtcoinAccount> GetOrCreateAccount(string userId)
        {
            var existing = await _db.Accounts
                .AsQueryable()
                .Include(x => x.Stats)
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.DeletedAt == null
                );

            if (existing != null)
            {
                return existing;
            }

            Log.Debug("Creating new account for {UserId}", userId);
            var account = new ButtcoinAccount
            {
                UserId = userId,
                Stats = new ButtcoinStats(),
            };
            await _db.Accounts.AddAsync(account);
            await _db.SaveChangesAsync();
            return account;
        }

        public async Task<ButtcoinAccount> MineCoin(IUser user, bool isBruteForce)
        {
            return await MineCoin(user.Id.ToString(), isBruteForce);
        }

        public async Task<ButtcoinAccount> MineCoin(string userId, bool isBruteForce)
        {
            Log.Debug("{UserId} mined a buttcoin (bruteforce? {IsBruteForce}", userId, isBruteForce);
            var account = await GetOrCreateAccount(userId);

            account.Balance += 1;
            account.Stats.AmountBruteforced += isBruteForce ? 1ul : 0ul;
            account.Stats.AmountMined += 1;

            await _db.SaveChangesAsync();

            return account;
        }

        public async Task<ButtcoinAccount> ActivateAccount(IUser user)
        {
            var account = await GetOrCreateAccount(user.Id.ToString());
            return await ActivateAccount(account);
        }

        public async Task<ButtcoinAccount> ActivateAccount(ButtcoinAccount account)
        {
            if (!account.IsActive)
            {
                Log.Debug("Activating {UserId}'s account", account.UserId);
                account.IsActive = true;
                await _db.SaveChangesAsync();
            }

            return account;
        }

        public async Task<(ButtcoinAccount, ButtcoinAccount)> Transfer(IUser fromUser, IUser toUser, ulong amount, string reason)
        {
            return await Transfer(fromUser.Id.ToString(), toUser.Id.ToString(), amount, reason);
        }

        public async Task<(ButtcoinAccount, ButtcoinAccount)> Transfer(string fromUserId, string toUserId, ulong amount, string reason)
        {
            if (fromUserId == toUserId)
            {
                Log.Debug("Failed to transfer {Amount} from {FromUserId} to {ToUserId} with reason '{Reason}', because they are the same person", amount, fromUserId, toUserId, reason);
                throw new SamePersonException();
            }

            var fromAccount = await GetOrCreateAccount(fromUserId);
            var toAccount = await GetOrCreateAccount(toUserId);

            if (amount > fromAccount.Balance)
            {
                Log.Debug("Failed to transfer {Amount} from {FromUserId} to {ToUserId} with reason '{Reason}', because of insufficient funds", amount, fromUserId, toUserId, reason);
                throw new NotEnoughFundsException();
            }

            if (!toAccount.IsActive)
            {
                Log.Debug("Failed to transfer {Amount} from {FromUserId} to {ToUserId} with reason '{Reason}', because the target doesn't have an active account", amount, fromUserId, toUserId, reason);
                throw new AccountNotActive();
            }

            Log.Debug("Transferred {Amount} from {FromUserId} to {ToUserId} with reason '{Reason}'", amount, fromUserId, toUserId, reason);

            fromAccount.Balance -= amount;
            toAccount.Balance += amount;

            fromAccount.Stats.AmountGifted += amount;
            toAccount.Stats.AmountReceived += amount;

            await _db.SaveChangesAsync();

            return (fromAccount, toAccount);
        }
    }
}
