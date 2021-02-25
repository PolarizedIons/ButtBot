using System.Threading;
using System.Threading.Tasks;
using ButtBot.Twitch.Services;
using Microsoft.Extensions.Hosting;

namespace ButtBot.Twitch
{
    public class App : IHostedService
    {
        private readonly TwitchService _twitchService;

        public App(TwitchService twitchService)
        {
            _twitchService = twitchService;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _twitchService.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _twitchService.Stop();
            return Task.CompletedTask;
        }
    }
}
