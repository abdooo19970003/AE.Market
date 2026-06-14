using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Catalog;

internal sealed class VariantAttributeValueConfiguration : IEntityTypeConfiguration<VariantAttributeValue>
{
    public void Configure(EntityTypeBuilder<VariantAttributeValue> builder)
    {
        builder.ToTable("variant_attribute_values", "catalog");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ValueText).HasMaxLength(2000);
        builder.Property(x => x.ValueDecimal).HasPrecision(18, 4);
    }
}
