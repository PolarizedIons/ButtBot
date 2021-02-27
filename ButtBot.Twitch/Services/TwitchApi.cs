using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ButtBot.Library.Extentions;
using Microsoft.Extensions.Configuration;
using Serilog;
using TwitchLib.Api;

namespace ButtBot.Twitch.Services
{
    public class TwitchApi : ISingletonDiService
    {
        private readonly List<string> _targetChannels;
        private readonly TwitchAPI _api;

        public TwitchApi(IConfiguration config)
        {
            _api = new TwitchAPI();
            _api.Settings.ClientId = config["Bot:ClientId"];
            _api.Settings.AccessToken = config["Bot:AccessToken"];

            var targetChannelNames = config["Bot:ChannelNames"];

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.twitchtv.v5+json"));
            httpClient.DefaultRequestHeaders.Add("Client-ID", config["Bot:ClientId"]);
            
            var res = httpClient.GetAsync($"https://api.twitch.tv/kraken/users?login={targetChannelNames}").Result;
            var json = res.Content.ReadAsStringAsync().Result;
            var userSearchResult = JsonSerializer.Deserialize<TwitchUserSearchDto>(json);
            _targetChannels = userSearchResult.Users.Select(x => x.Id.ToString()).ToList();
        }

        public async Task<Dictionary<string, bool>> IsTargetsLive()
        {
            Log.Debug("Fetching live channels...");
            var stream = await _api.V5.Streams.GetLiveStreamsAsync(_targetChannels);

            if (stream == null || stream.Streams.Length == 0)
            {
                return _targetChannels.ToDictionary(x => x, x => false);
            }

            return _targetChannels.ToDictionary(x => x, x => stream.Streams.Any(s => s.Channel.Id == x));
        }
    }

    public class TwitchUserSearchDto
    {
        [JsonPropertyName("users")]
        public IEnumerable<TwitchUserDto> Users { get; set; } = null!;
    }
    
    public class TwitchUserDto
    {
        [JsonPropertyName("_id")]
        public string InternalId { get; set; } = "0";

        public uint Id => uint.Parse(InternalId);

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = null!;
    }
}
