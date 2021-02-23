using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using ButtBot.Library.Extentions;
using ButtBot.Website.Models;

namespace ButtBot.Website.Services
{
    public class DiscordService : ISingletonDiService
    {
        public async Task<(UserInfo?, IEnumerable<Connection>?)> GetUserInfo(string userToken)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://discord.com/api/"); // NOTE THE TRAILING SLASH!
            httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse("Bearer " + userToken);

            var userInfoResp = await httpClient.GetAsync("users/@me");
            if (!userInfoResp.IsSuccessStatusCode)
            {
                return (null, null);
            }

            var userInfo = JsonSerializer.Deserialize<UserInfo>(await userInfoResp.Content.ReadAsStringAsync());

            var connectionsResp = await httpClient.GetAsync("users/@me/connections");
            if (!userInfoResp.IsSuccessStatusCode)
            {
                return (userInfo, null);
            }
            var connections = JsonSerializer.Deserialize<IEnumerable<Connection>>(await connectionsResp.Content.ReadAsStringAsync());

            return (userInfo, connections);
        }
    }
}
