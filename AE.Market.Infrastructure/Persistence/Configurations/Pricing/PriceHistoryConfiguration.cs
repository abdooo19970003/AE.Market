using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Pricing;

internal sealed class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistory>
{
    public void Configure(EntityTypeBuilder<PriceHistory> builder)
    {
        builder.ToTable("price_history", "pricing");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PriceId).IsRequired();
        builder.Property(x => x.VariantId).IsRequired();

        builder.OwnsOne(x => x.OldAmount, oldBuilder =>
        {
            oldBuilder.Property(m => m.Amount)
                .HasColumnName("old_amount")
                .HasPrecision(18, 4)
                .IsRequired();

            oldBuilder.Property(m => m.Currency)
                .HasColumnName("old_currency")
                .HasMaxLength(3)
                .HasConversion(
                    v => v.Code,
                    v => Domain.Common.ValueObjects.Currency.FromCode(v))
                .IsRequired();
        });

        builder.OwnsOne(x => x.NewAmount, newBuilder =>
        {
            newBuilder.Property(m => m.Amount)
                .HasColumnName("new_amount")
                .HasPrecision(18, 4)
                .IsRequired();

            newBuilder.Property(m => m.Currency)
                .HasColumnName("new_currency")
                .HasMaxLength(3)
                .HasConversion(
                    v => v.Code,
                    v => Domain.Common.ValueObjects.Currency.FromCode(v))
                .IsRequired();
        });

        builder.Property(x => x.Reason).HasConversion<int>().IsRequired();
        builder.Property(x => x.ChangedBy);
        builder.Property(x => x.ChangedAt).IsRequired();

        builder.HasIndex(x => x.VariantId);
        builder.HasIndex(x => x.PriceId);
    }
}
