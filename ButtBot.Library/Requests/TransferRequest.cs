using RMQCommandService.Models;

namespace ButtBot.Library.Requests
{
    public class TransferRequest : ICommand
    {
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public ulong Amount { get; set; }
        public string Reason { get; set; }
    }
}
