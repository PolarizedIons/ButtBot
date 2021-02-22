namespace ButtBot.Library.Models.Database
{
    public class HolderAccount : DbEntity
    {
        public string UserId { get; set; }
        public BotPlatform Platform { get; set; }
        public ulong AmountMined { get; set; }
        public ulong AmountBruteforced { get; set; }
    }
}
