using RMQCommandService.Models;

namespace ButtBot.Library.Requests
{
    public class ActivateAccountRequest : ICommand
    {
        public string UserId { get; set; }
    }
}
