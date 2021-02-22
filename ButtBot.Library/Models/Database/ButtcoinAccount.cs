namespace ButtBot.Library.Models.Database
{
    public class ButtcoinAccount : DbEntity
    {
        public string DiscordUserId { get; set; }
        public ulong Balance { get; set; }
        public bool IsActive { get; set; }

        public ButtcoinStats Stats { get; set; }
    }
}
