using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ButtBot.Database;
using ButtBot.Extentions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace ButtBot
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {

            var loggerBuilder = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext();

            if (Debugger.IsAttached)
            {
                loggerBuilder.WriteTo.Console();
            }
            else
            {
                loggerBuilder.WriteTo.Console(new JsonFormatter());
                
            }
            Log.Logger = loggerBuilder.CreateLogger();

            try
            {
                using var host = CreateHostBuilder(args).Build();
                await host.StartAsync();
                await host.WaitForShutdownAsync();
                await host.StopAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal exception");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostCtx, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", true)
                        .AddJsonFile("appsettings.Development.json", true)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices((hostCtx, services) =>
                {
                    services.AddDbContext<DatabaseContext>(opts =>
                    {
                        var connectionString = hostCtx.Configuration.GetConnectionString("ButtBot");
                        opts.UseMySql(hostCtx.Configuration.GetConnectionString("ButtBot"), ServerVersion.AutoDetect(connectionString));
                    });

                    services.AddSingleton(new DiscordSocketClient(
                        new DiscordSocketConfig
                        {
                            MessageCacheSize = 100,
                            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildEmojis | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages,
                            AlwaysDownloadUsers = true
                        }
                    ));

                    services.AddSingleton(new CommandService(new CommandServiceConfig
                    {
                        // DefaultRunMode = RunMode.Async,
                        LogLevel = LogSeverity.Verbose,
                        CaseSensitiveCommands = false
                    }));

                    services.DiscoverAndMakeDiServicesAvailable();

                    services.AddHostedService<App>();
                })
                .UseSerilog()
                .UseConsoleLifetime();
        }
    }
}
