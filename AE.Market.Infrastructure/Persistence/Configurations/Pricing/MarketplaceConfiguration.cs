using AE.Market.Domain.Aggregates.Pricing;
using AE.Market.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Pricing;

internal sealed class MarketplaceConfiguration : IEntityTypeConfiguration<Marketplace>
{
    public void Configure(EntityTypeBuilder<Marketplace> builder)
    {
        builder.ToTable("marketplaces", "pricing");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Region).HasMaxLength(100).IsRequired();

        builder.Property(x => x.PreferredCurrency)
            .HasColumnName("preferred_currency")
            .HasMaxLength(3)
            .HasConversion(
                v => v.Code,
                v => Currency.FromCode(v))
            .IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ix_marketplaces_code");
    }
}
