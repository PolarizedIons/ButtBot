using System.Collections.Generic;
using System.Threading.Tasks;
using ButtBot.Library.Extentions;
using ButtBot.Library.Requests;
using RMQCommandService.RabbitMq;

namespace ButtBot.Twitch.Services
{
    public class ConfigurationService : ISingletonDiService
    {
        private readonly RMQBus _bus;

        public ConfigurationService(RMQBus bus)
        {
            _bus = bus;
        }

        public async Task<List<string>> GetButtWords()
        {
            return await _bus.Send<List<string>>(new ButtWordsRequest());
        }
    }
}
