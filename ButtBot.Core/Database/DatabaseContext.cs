using System;
using ButtBot.Library.Models.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ButtBot.Core.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<ButtcoinAccount> Accounts { get; set; } = null!;
        public DbSet<HolderAccount> HolderAccounts { get; set; } = null!;
        public DbSet<ButtcoinStats> Stats { get; set; } = null!;
        public DbSet<LinkedAccounts> LinkedAccounts { get; set; } = null!;

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            ChangeTracker.StateChanged += OnEntityStateChanged;
            ChangeTracker.Tracked += OnEntityTracked;
        }

        private void OnEntityStateChanged(object? sender, EntityStateChangedEventArgs e)
        {
            if (e.NewState == EntityState.Modified && e.Entry.Entity is DbEntity entity)
            {
                entity.ModifiedAt = DateTime.UtcNow;
            }
        }

        private void OnEntityTracked(object? sender, EntityTrackedEventArgs e)
        {
            if (e.Entry.State == EntityState.Added && e.Entry.Entity is DbEntity entity)
            {
                entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;
                entity.ModifiedAt = DateTime.UtcNow;
            }
        }
    }
}
