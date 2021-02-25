using System;

namespace ButtBot.Library.Models.Database
{
    public class ButtcoinStats : DbEntity
    {
        public Guid AccountId { get; set; }
        public ButtcoinAccount Account { get; set; }

        public ulong AmountMined { get; set; }
        public ulong AmountBruteforced { get; set; }
        public ulong AmountReceived { get; set; }
        public ulong AmountGifted { get; set; }
    }
}
