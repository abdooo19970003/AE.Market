using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Catalog;

internal sealed class VariantImageConfiguration : IEntityTypeConfiguration<VariantImage>
{
    public void Configure(EntityTypeBuilder<VariantImage> builder)
    {
        builder.ToTable("variant_images", "catalog");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.AltText).HasMaxLength(500);
        builder.Property(x => x.IsPrimary).HasDefaultValue(false);
        builder.Property(x => x.SortOrder).HasDefaultValue(0);
    }
}
