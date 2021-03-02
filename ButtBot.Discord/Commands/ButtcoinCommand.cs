using System;
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
        private readonly string _linkUrl;

        public ButtcoinCommand(ButtcoinService buttcoinService, IConfiguration config)
        {
            _buttcoinService = buttcoinService;
            _logoUrl = config["Bot:LogoUrl"];
            _linkUrl = config["Bot:LinkUrl"];
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

            var embed = EmbedUtils.CreateTransferEmbed((IGuildUser) Context.User, toUser, fromAccount, toAccount, amount, reason, _logoUrl);
            await ReplyAsync(embed: embed.Build());
        }

        [Command("buttcoin tip")]
        [Summary("Transfer buttcoins from your account to someone else's")]
        public async Task Transfer(IGuildUser toUser, [Remainder] string reason = "Tip.")
        {
            var fromUser = (IGuildUser)Context.Message.Author;
            await _buttcoinService.ActivateAccount(fromUser);

            var (fromAccount, toAccount) = await _buttcoinService.Transfer(Context.Message.Author, toUser, 10, reason);

            var embed = EmbedUtils.CreateTransferEmbed((IGuildUser) Context.User, toUser, fromAccount, toAccount, 10, reason, _logoUrl);
            await ReplyAsync(embed: embed.Build());
        }

        [Command("buttcoin link")]
        [Summary("Link discord-connections to your buttcoin account")]
        public async Task Link()
        {
            var host = new Uri(_linkUrl).Host;

            var embed = new EmbedBuilder()
                .WithTitle($"Linking your buttcoin account - {host}")
                .WithThumbnailUrl(_logoUrl)
                .WithDescription("Link your discord connections to your buttcoin account, earning buttcoins wherever you go!")
                .WithUrl(_linkUrl)
                .WithColor(Color.Green)
                .WithFooter("ButtBot", _logoUrl)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }
    }
}
