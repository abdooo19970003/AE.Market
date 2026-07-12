using AE.Market.Domain.Aggregates.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Inventory;

internal sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("inventory_items", "inventory");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.VariantId).IsRequired();
        builder.Property(x => x.WarehouseId).IsRequired();
        builder.Property(x => x.StockQuantity).HasDefaultValue(0);
        builder.Property(x => x.ReservedQuantity).HasDefaultValue(0);
        builder.Property(x => x.TrackInventory).HasDefaultValue(true);
        builder.Property(x => x.AllowBackorder).HasDefaultValue(false);
        builder.Property(x => x.LowStockThreshold).HasDefaultValue(0);
        builder.Property(x => x.RowVersion).HasColumnType("bytea");

        builder.HasIndex(x => x.VariantId).IsUnique();

        builder.OwnsOne(x => x.ShippingDimensions, sd =>
        {
            sd.Property(d => d.WeightInGrams).HasColumnName("shipping_weight_grams");
            sd.Property(d => d.LongInCentimeter).HasColumnName("shipping_length_cm");
            sd.Property(d => d.WidthInCentimeter).HasColumnName("shipping_width_cm");
            sd.Property(d => d.HeightInCentimeter).HasColumnName("shipping_height_cm");
        });
    }
}
