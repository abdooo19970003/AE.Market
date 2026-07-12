using AE.Market.Domain.Aggregates.Pricing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Pricing;

internal sealed class MarketplaceTaxRateConfiguration : IEntityTypeConfiguration<MarketplaceTaxRate>
{
    public void Configure(EntityTypeBuilder<MarketplaceTaxRate> builder)
    {
        builder.ToTable("marketplace_tax_rates", "pricing");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MarketplaceId).IsRequired();
        builder.Property(x => x.TaxCodeId).IsRequired();
        builder.Property(x => x.TaxRate).HasPrecision(5, 4).IsRequired();
        builder.Property(x => x.ValidFrom);
        builder.Property(x => x.ValidTo);

        builder.HasIndex(x => new { x.MarketplaceId, x.TaxCodeId, x.ValidTo })
            .HasFilter("\"ValidTo\" IS NULL")
            .IsUnique()
            .HasDatabaseName("ix_marketplace_tax_rates_active_per_marketplace_taxcode");
    }
}
