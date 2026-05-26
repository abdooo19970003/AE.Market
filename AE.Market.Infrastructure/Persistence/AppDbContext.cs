using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AE.Market.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        // Outbox Schema
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        // Auth Schema
        public DbSet<User> Users { get; set; }
        public DbSet<UserPermission> Permissions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }


       

    }
}
