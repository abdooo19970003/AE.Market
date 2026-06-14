using AE.Market.Domain.Aggregates.Catalog.Units;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Catalog;

internal sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("units", "catalog");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UnitName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Abbreviation).HasMaxLength(20).IsRequired();
        builder.Property(x => x.IsBaseUnit).HasDefaultValue(true);
        builder.Property(x => x.ExchangeRateToBaseUnit).HasPrecision(18, 6).HasDefaultValue(1m);
    }
}
