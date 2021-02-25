using ButtBot.Library.Models;
using RMQCommandService.Models;

namespace ButtBot.Library.Requests
{
    public class LinkAccountRequest : ICommand
    {
        public BotPlatform Platform { get; set; }
        public string PlatformId { get; set; }
        public string DiscordId { get; set; }
    }
}
