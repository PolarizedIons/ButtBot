using System.Threading;
using System.Threading.Tasks;
using ButtBot.Commands;
using ButtBot.Database;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ButtBot
{
    public class App : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly DiscordSocketClient _discord;
        private readonly CommandHandler _commandHandler;

        public App(IServiceScopeFactory scopeFactory, IConfiguration config, DiscordSocketClient discord, CommandHandler commandHandler)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _discord = discord;
            _commandHandler = commandHandler;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                Log.Information("Migrating Database...");
                await db.Database.MigrateAsync(cancellationToken: cancellationToken);
            }

            Log.Debug("Initializing command handler");
            await _commandHandler.InitializeAsync();

            Log.Information("Logging in...");
            await _discord.LoginAsync(TokenType.Bot, _config["Bot:Token"]);
            await _discord.StartAsync();
            Log.Information("Bot started");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discord.StopAsync();
            await _discord.LogoutAsync();
        }
    }
}
