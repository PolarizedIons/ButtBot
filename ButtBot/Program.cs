﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ButtBot.Database;
using ButtBot.Extentions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace ButtBot
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            SetupLogging();

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
                        opts.ConfigureWarnings(c => c.Log(
                            (RelationalEventId.CommandExecuted, LogLevel.Debug),
                            (CoreEventId.ContextInitialized, LogLevel.Debug)
                        ));
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

        private static void SetupLogging()
        {
            var logDir = Environment.GetEnvironmentVariable("LOG_DIR") ?? "./logs/";
            var loggerBuilder = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(new JsonFormatter(), Path.Join(logDir, "log.txt"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30);

            if (Debugger.IsAttached)
            {
                loggerBuilder.WriteTo.Console();
            }
            else
            {
                loggerBuilder.WriteTo.Console(new JsonFormatter());
            }

            var seqUrl = Environment.GetEnvironmentVariable("SEQ_URL");
            var seqApiKey = Environment.GetEnvironmentVariable("SEQ_API");
            if (!string.IsNullOrEmpty(seqUrl))
            {
                loggerBuilder.WriteTo.Seq(seqUrl, apiKey: seqApiKey);
            }

            Log.Logger = loggerBuilder.CreateLogger();
        }
    }
}
