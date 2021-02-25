using System.Threading;
using System.Threading.Tasks;
using ButtBot.Core.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ButtBot.Core
{
    public class App : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public App(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                Log.Information("Migrating Database...");
                await db.Database.MigrateAsync(cancellationToken: cancellationToken);
            }
            
            Log.Information("Core started :)");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
