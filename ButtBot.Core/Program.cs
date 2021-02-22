using System;
using System.IO;
using System.Threading.Tasks;
using ButtBot.Core.Database;
using ButtBot.Library;
using ButtBot.Library.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RMQCommandService.Extentions;
using RMQCommandService.Models;
using Serilog;

namespace ButtBot.Core
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            LogHelper.SetupLogging();

            Log.Information("Staring ButtBot Core");
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

                    services.AddRMQCommandService(options =>
                    {
                        options.Host = hostCtx.Configuration["RabbbitMq:Host"];
                        options.User = hostCtx.Configuration["RabbbitMq:User"];
                        options.Password = hostCtx.Configuration["RabbbitMq:Password"];
                        options.ReceiveServiceId = "CORE";
                    });
                    services.ConfigureRMQHandler(HandlerServiceType.Scoped);
                    services.DiscoverAndMakeDiServicesAvailable();
                    services.AddHostedService<App>();
                })
                .UseSerilog()
                .UseConsoleLifetime();
        }
    }
}
