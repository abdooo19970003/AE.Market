using AE.Market.Domain.Aggregates.Catalog.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Catalog;

internal sealed class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        builder.ToTable("product_attribute_values", "catalog", tb =>
            tb.HasCheckConstraint("CK_product_attribute_values_single_owner",
                "(\"ProductId\" IS NOT NULL AND \"VariantId\" IS NULL) OR (\"ProductId\" IS NULL AND \"VariantId\" IS NOT NULL)"));
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AttributeId).IsRequired();
        builder.Property(x => x.VariantId);

        builder.Property(x => x.IsVariantDefiner).HasDefaultValue(false);

        builder.Property(x => x.ValueText).HasMaxLength(2000);
        builder.Property(x => x.ValueInteger);
        builder.Property(x => x.ValueDecimal).HasPrecision(18, 4);
        builder.Property(x => x.ValueBoolean);
        builder.Property(x => x.ValueDateTime);
        builder.Property(x => x.ValueOptionId);

        builder.HasIndex(x => new { x.ProductId, x.AttributeId })
            .IsUnique()
            .HasFilter("\"ProductId\" IS NOT NULL");

        builder.HasIndex(x => new { x.VariantId, x.AttributeId })
            .IsUnique()
            .HasFilter("\"VariantId\" IS NOT NULL");
    }
}
