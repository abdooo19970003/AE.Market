using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Catalog;

internal sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants", "catalog");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Sku)
            .HasConversion(v => v.Value, v => Sku.Create(v))
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(ProductStatus.Active);
        builder.Property(x => x.ListPrice).HasPrecision(18, 4);
        builder.Property(x => x.StockQuantity).HasDefaultValue(0);
        builder.Property(x => x.ReservedQuantity).HasDefaultValue(0);
        builder.Property(x => x.RowVersion).HasColumnType("bytea");
        builder.HasIndex(x => x.Sku).IsUnique();

        builder.HasMany(x => x.AttributeValues)
            .WithOne()
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Images)
            .WithOne()
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
