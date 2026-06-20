using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AE.Market.Domain.Tests.EfCore;

/// <summary>
/// InMemory provider has known limitations with RowVersion/IsRowVersion().
/// It can't correctly detect new child entities as Added when they have non-temporary keys.
/// These tests prove the original bug: calling Update() on a tracked aggregate with a new child
/// causes DbUpdateConcurrencyException. Real PostgreSQL handles this correctly without Update().
/// </summary>
public sealed class AddVariantHandlerFlowTests
{
    /// <summary>
    /// Proves that WITHOUT .Include() + without Update():
    /// InMemory marks the new variant as Modified (not Added).
    /// This is an InMemory limitation — real providers handle this correctly.
    /// </summary>
    [Fact]
    public void WithoutInclude_VariantIsMarkedModified()
    {
        var productId = Guid.NewGuid();
        using var db = CreateDbContext();

        db.Products.Add(BuildProduct(productId));
        db.SaveChanges();
        db.ChangeTracker.Clear();

        var loaded = db.Products.Find(productId)!;
        loaded.AddVariant(Guid.NewGuid(), "Black 256GB", "XI-12P-256-BLK");
        db.ChangeTracker.DetectChanges();

        var variantEntry = db.Entry(loaded.Variants.Single());
        variantEntry.State.Should().Be(EntityState.Modified);
    }

    /// <summary>
    /// Proves that calling Update() on a tracked product with new variant
    /// causes DbUpdateConcurrencyException (the original bug).
    /// </summary>
    [Fact]
    public void UpdateOnTrackedProduct_ThrowsConcurrency()
    {
        var productId = Guid.NewGuid();
        using var db = CreateDbContext();

        db.Products.Add(BuildProduct(productId));
        db.SaveChanges();
        db.ChangeTracker.Clear();

        var loaded = db.Products.Find(productId)!;
        loaded.AddVariant(Guid.NewGuid(), "Black 256GB", "XI-12P-256-BLK");

        var act = () =>
        {
            db.Products.Update(loaded);
            db.SaveChanges();
        };

        act.Should().Throw<DbUpdateConcurrencyException>();
    }

    private static Product BuildProduct(Guid id)
    {
        return Product.Create(id, "Xiaomi 12 Pro", "xiaomi-12-pro", "XI-12P-256-BLK", Guid.NewGuid(), ProductType.Configurable);
    }

    private static TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductVariant> Variants => Set<ProductVariant>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Sku)
                    .HasConversion(v => v.Value, v => Sku.Create(v));
                e.Property(x => x.Slug)
                    .HasConversion(v => v.Value, v => Slug.Create(v));
                e.Ignore(x => x.SalePrice);
                e.Ignore(x => x.StockQuantity);
                e.Ignore(x => x.Images);
                e.Ignore(x => x.Tags);
                e.Ignore(x => x.AttributeValues);
                e.Ignore(x => x.Relations);
                e.Ignore(x => x.BundleItems);
                e.HasMany(x => x.Variants)
                    .WithOne()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProductVariant>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Sku)
                    .HasConversion(v => v.Value, v => Sku.Create(v));
                e.Ignore(x => x.AttributeValues);
                e.Ignore(x => x.Images);
                e.Property(x => x.RowVersion).IsRowVersion();
            });
        }
    }
}
