namespace ButtBot.Library.Models.Database
{
    public class LinkedAccounts : DbEntity
    {
        public string DiscordId { get; set; }
        public BotPlatform Platform { get; set; }
        public string PlatformId { get; set; }
    }
}
