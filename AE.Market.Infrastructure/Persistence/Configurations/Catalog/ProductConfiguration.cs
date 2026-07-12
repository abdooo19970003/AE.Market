using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Catalog;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", "catalog");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Slug)
            .HasConversion(v => v.Value, v => Slug.Create(v))
            .HasMaxLength(300)
            .IsRequired();
        builder.Property(x => x.Details).HasMaxLength(4000);
        builder.Property(x => x.ShortDescription).HasMaxLength(1000);
        builder.Property(x => x.LongDescription).HasMaxLength(4000);
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(ProductStatus.Active);
        builder.Property(x => x.AllowBackOrder).HasDefaultValue(false);
        builder.Property(x => x.ProductType).HasConversion<int>().IsRequired();
        builder.Property(x => x.MetaTitle).HasMaxLength(200);
        builder.Property(x => x.MetaDescription).HasMaxLength(500);
        builder.Property(x => x.MetaKeywords).HasMaxLength(500);
        builder.Property(x => x.TaxCodeId).IsRequired();

        builder.Property(x => x.Sku)
            .HasConversion(v => v.Value, v => Sku.Create(v))
            .HasMaxLength(50)
            .IsRequired();

        builder.Ignore(x => x.ListPrice);
        builder.Ignore(x => x.StockQuantity);

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.Sku).IsUnique();

        builder.HasMany(x => x.Variants)
            .WithOne()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Images)
            .WithOne()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Tags)
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AttributeValues)
            .WithOne()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.BundleItems)
            .WithOne()
            .HasForeignKey(x => x.BundleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Relations)
            .WithOne()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
