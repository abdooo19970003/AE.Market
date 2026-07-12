using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Catalog.Units;
using AE.Market.Domain.Aggregates.Cart;
using AE.Market.Domain.Aggregates.Orders;
using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Aggregates.Pricing;
using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
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
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<VariantImage> VariantImages { get; set; }
        public DbSet<BundleItem> BundleItems { get; set; }
        public DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }
        public DbSet<AttributeGroup> AttributeGroups { get; set; }
        public DbSet<ProductRelation> ProductRelations { get; set; }

        // Pricing Schema
        public DbSet<Price> Prices { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<Marketplace> Marketplaces { get; set; }
        public DbSet<MarketplaceTaxRate> MarketplaceTaxRates { get; set; }

        // Cart Schema
        public DbSet<Domain.Aggregates.Cart.Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        // Orders Schema
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<IdempotencyRequest> IdempotencyRequests { get; set; }

        // Inventory Schema
        public DbSet<InventoryItem> InventoryItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)
                    && entityType.GetDeclaredQueryFilters().Count == 0)
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var body = Expression.Equal(
                        Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
                        Expression.Constant(false));
                    entityType.SetQueryFilter(Expression.Lambda(body, parameter));
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
