using ButtBot.Library.Models;
using RMQCommandService.Models;

namespace ButtBot.Library.Requests
{
    public class MineCoinRequest : ICommand
    {
        public string UserId { get; set; }
        public BotPlatform Platform { get; set; }
        public bool IsBruteForce { get; set; }
    }
}

