using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ButtBot.Library.Extentions;
using ButtBot.Library.Models;
using ButtBot.Library.Requests;
using ButtBot.Website.Models;
using RMQCommandService.Models;
using RMQCommandService.RabbitMq;

namespace ButtBot.Website.Services
{
    public class ButtcoinService : ISingletonDiService
    {
        private static readonly Dictionary<string, BotPlatform> Platforms = new Dictionary<string, BotPlatform>();
        
        private readonly RMQBus _bus;

        public ButtcoinService(RMQBus bus)
        {
            _bus = bus;

            // If we ever support more platforms, this is where to add them
            Platforms.Add("twitch", BotPlatform.Twitch);
        }

        public async Task<IEnumerable<Connection>> LinkConnections(UserInfo userInfo, IEnumerable<Connection> connections)
        {
            var filteredConnections = connections.Where(x => Platforms.Keys.Contains(x.Type)).ToArray();

            foreach (var connection in filteredConnections)
            {
                await _bus.Send<EmptyResponse>(new LinkAccountRequest
                {
                    Platform = Platforms[connection.Type],
                    PlatformId = connection.Id,
                    DiscordId = userInfo.Id,
                });
            }

            return filteredConnections;
        }
    }
}
