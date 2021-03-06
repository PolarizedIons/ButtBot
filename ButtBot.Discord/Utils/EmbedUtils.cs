using ButtBot.Discord.Extentions;
using ButtBot.Library.Models.Database;
using Discord;

namespace ButtBot.Discord.Utils
{
    public static class EmbedUtils
    {
        public static EmbedBuilder CreateTransferEmbed(IGuildUser fromUser, IGuildUser toUser, ButtcoinAccount fromAccount, ButtcoinAccount toAccount, ulong amount, string reason, string logo)
        {
            return new EmbedBuilder()
                .WithTitle("Buttcoin transfer report  ➡")
                .WithThumbnailUrl(logo)
                .AddField($"From: {fromUser.GetDisplayName()}", $"{fromAccount.Balance} buttcoins", true)
                .AddField($"To: {toUser.GetDisplayName()}", $"{toAccount.Balance} buttcoins", true)
                .AddField("Amount", $"{amount} buttcoins", true)
                .AddField("Reason", reason, true)
                .WithColor(Color.Green)
                .WithFooter("ButtBot", logo)
                .WithCurrentTimestamp();
        }

        public static EmbedBuilder CreateBalanceEmbed(IGuildUser user, ButtcoinAccount account, string logo)
        {
            return new EmbedBuilder()
                .WithTitle($"{user.GetDisplayName()}'s buttcoin balance")
                .WithThumbnailUrl(user.GetAvatarUrl())
                .AddField("Account Status", account.IsActive ? "Active" : "Inactive", true)
                .AddField("Balance", $"{account.Balance} buttcoins", true)
                .WithColor(Color.Green)
                .WithFooter("ButtBot", logo)
                .WithCurrentTimestamp();
        }

        public static EmbedBuilder CreateStatsEmbed(IGuildUser user, ButtcoinAccount account, string logo)
        {
            return CreateBalanceEmbed(user, account, logo)
                .WithTitle($"{user.GetDisplayName()}'s buttcoin stats")
                .AddField("# Mined", $"{account.Stats.AmountMined} buttcoins", true)
                .AddField("# Bruteforced", $"{account.Stats.AmountBruteforced} buttcoins", true)
                .AddField("# Gifted", $"{account.Stats.AmountGifted} buttcoins", true)
                .AddField("# Received", $"{account.Stats.AmountReceived} buttcoins", true);
        }
    }
}
