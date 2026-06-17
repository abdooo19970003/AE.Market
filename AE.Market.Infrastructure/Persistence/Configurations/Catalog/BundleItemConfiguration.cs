using AE.Market.Domain.Aggregates.Catalog.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Catalog;

internal sealed class BundleItemConfiguration : IEntityTypeConfiguration<BundleItem>
{
    public void Configure(EntityTypeBuilder<BundleItem> builder)
    {
        builder.ToTable("bundle_items", "catalog");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BundleId).IsRequired();
        builder.Property(x => x.ItemId).IsRequired();
        builder.Property(x => x.Quantity).IsRequired().HasDefaultValue(1);
    }
}
