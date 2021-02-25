using System;
using System.IO;
using System.Threading.Tasks;
using ButtBot.Library;
using ButtBot.Library.Extentions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RMQCommandService.Extentions;
using Serilog;

namespace ButtBot.Twitch
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            LogHelper.SetupLogging();

            Log.Information("Staring ButtBot TwitchBot");
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
                    services.AddRMQCommandService(options =>
                    {
                        options.Host = hostCtx.Configuration["RabbbitMq:Host"];
                        options.User = hostCtx.Configuration["RabbbitMq:User"];
                        options.Password = hostCtx.Configuration["RabbbitMq:Password"];
                        options.DefaultBusService = "CORE";
                    });

                    services.DiscoverAndMakeDiServicesAvailable();
                    services.AddHostedService<App>();
                })
                .UseSerilog()
                .UseConsoleLifetime();
        }
    }
}
