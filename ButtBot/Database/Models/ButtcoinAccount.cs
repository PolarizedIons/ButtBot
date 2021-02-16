using System.ComponentModel.DataAnnotations.Schema;

namespace ButtBot.Database.Models
{
    public class ButtcoinAccount : DbEntity
    {
        public string UserId { get; set; }
        public ulong Balance { get; set; }
        public bool IsActive { get; set; }

        public ButtcoinStats Stats { get; set; }
    }
}
