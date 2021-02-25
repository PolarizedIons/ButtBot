using RMQCommandService.Models;

namespace ButtBot.Library.Requests
{
    public class GetOrCreateAccountRequest : ICommand
    {
        public string UserId { get; set; }
    }
}
