using System.Threading.Tasks;
using ButtBot.Discord.Services;
using ButtBot.Discord.Utils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace ButtBot.Discord.Commands
{
    public class ButtcoinCommand : ModuleBase
    {
        private readonly ButtcoinService _buttcoinService;
        private readonly string _logoUrl;

        public ButtcoinCommand(ButtcoinService buttcoinService, IConfiguration config)
        {
            _buttcoinService = buttcoinService;
            _logoUrl = config["Bot:LogoUrl"];
        }

        [Command("buttcoin balance")]
        [Alias("buttcoin bal")]
        [Summary("Get account balance for yourself/someone else")]
        public async Task Balance(IUser? user = null)
        {
            if (user != null && !(user is SocketGuildUser))
            {
                return;
            }

            var guildUser = (IGuildUser) (user ?? Context.User);

            var account = await _buttcoinService.GetOrCreateAccount(guildUser, true);

            var embed = EmbedUtils.CreateBalanceEmbed(guildUser, account, _logoUrl);
            await ReplyAsync(embed: embed.Build());
        }

        [Command("buttcoin stats")]
        [Summary("Get Stats for yourself/someone else")]
        public async Task Stats(IUser? user = null)
        {
            if (user != null && !(user is SocketGuildUser))
            {
                return;
            }

            var guildUser = (IGuildUser) (user ?? Context.User);

            var account = await _buttcoinService.GetOrCreateAccount(guildUser, user == null);

            var embed = EmbedUtils.CreateStatsEmbed(guildUser, account, _logoUrl);
            await ReplyAsync(embed: embed.Build());
        }

        [Command("buttcoin transfer")]
        [Summary("Transfer buttcoins from your account to someone else's")]
        public async Task Transfer(IGuildUser toUser, ulong amount, [Remainder] string reason = "No reason.")
        {
            var fromUser = (IGuildUser)Context.Message.Author;
            await _buttcoinService.ActivateAccount(fromUser);

            var (fromAccount, toAccount) = await _buttcoinService.Transfer(Context.Message.Author, toUser, amount, reason);

            var embed = EmbedUtils.CreateTransferEmbed((IGuildUser) Context.User, toUser, fromAccount, toAccount, reason, _logoUrl);
            await ReplyAsync(embed: embed.Build());
        }

        [Command("buttcoin tip")]
        [Summary("Transfer buttcoins from your account to someone else's")]
        public async Task Transfer(IGuildUser toUser)
        {
            var fromUser = (IGuildUser)Context.Message.Author;
            await _buttcoinService.ActivateAccount(fromUser);

            var (fromAccount, toAccount) = await _buttcoinService.Transfer(Context.Message.Author, toUser, 10, "Tip.");

            var embed = EmbedUtils.CreateTransferEmbed((IGuildUser) Context.User, toUser, fromAccount, toAccount, "Tip.", _logoUrl);
            await ReplyAsync(embed: embed.Build());
        }
    }
}
