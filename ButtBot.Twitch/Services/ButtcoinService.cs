using System.Threading.Tasks;
using ButtBot.Library.Extentions;
using ButtBot.Library.Models;
using ButtBot.Library.Requests;
using RMQCommandService.Models;
using RMQCommandService.RabbitMq;

namespace ButtBot.Twitch.Services
{
    public class ButtcoinService : ISingletonDiService
    {
        private readonly RMQBus _bus;

        public ButtcoinService(RMQBus bus)
        {
            _bus = bus;
        }
        
        public async Task MineCoin(string userId, bool isBruteForce)
        {
            await _bus.Send<EmptyResponse>(new MineCoinRequest
            {
                UserId = userId,
                IsBruteForce = isBruteForce,
                Platform = BotPlatform.Twitch,
            });
        }
    }
}
