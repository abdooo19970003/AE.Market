using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Pricing;

internal sealed class PriceConfiguration : IEntityTypeConfiguration<Price>
{
    public void Configure(EntityTypeBuilder<Price> builder)
    {
        builder.ToTable("prices", "pricing");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.VariantId).IsRequired();
        builder.Property(x => x.MarketplaceId);
        builder.Property(x => x.Type).HasConversion<int>().IsRequired();

        builder.OwnsOne(x => x.PriceAmount, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("amount")
                .HasPrecision(18, 4)
                .IsRequired();

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .HasConversion(
                    v => v.Code,
                    v => Domain.Common.ValueObjects.Currency.FromCode(v))
                .IsRequired();
        });

        builder.Property(x => x.ValidFrom);
        builder.Property(x => x.ValidTo);

        builder.HasIndex(x => new { x.VariantId, x.Type, x.ValidTo })
            .HasFilter("\"ValidTo\" IS NULL")
            .IsUnique()
            .HasDatabaseName("ix_prices_active_price_per_variant_type");
    }
}
