using System.Collections.Generic;
using System.Threading.Tasks;
using ButtBot.Library.Extentions;
using Microsoft.Extensions.Configuration;
using TwitchLib.Api;

namespace ButtBot.Twitch.Services
{
    public class TwitchApi : ISingletonDiService
    {
        public readonly TwitchAPI _api;
        private readonly string _targetChannel;

        public TwitchApi(IConfiguration config)
        {
            _api = new TwitchAPI();
            _api.Settings.ClientId = config["Bot:ClientId"];
            _api.Settings.AccessToken = config["Bot:AccessToken"];
            _targetChannel = config["Bot:ChannelName"];
        }

        public async Task<bool> IsTargetLive()
        {
            var stream = await _api.V5.Streams.GetLiveStreamsAsync(new List<string> {_targetChannel});
            return stream?.Streams.Length == 1;
        }
    }
}
