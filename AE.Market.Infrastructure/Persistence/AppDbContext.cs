using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Catalog.Units;
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

        // Catalog Schema
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductTaxCode> ProductTaxCodes { get; set; }
        public DbSet<GroupUnit> GroupUnits { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<CategoryAttribute> CategoryAttributes { get; set; }
        public DbSet<AttributeOption> AttributeOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
