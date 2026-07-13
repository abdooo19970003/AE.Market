using AE.Market.Domain.Aggregates.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Orders;

internal sealed class IdempotencyRequestConfiguration : IEntityTypeConfiguration<IdempotencyRequest>
{
    public void Configure(EntityTypeBuilder<IdempotencyRequest> builder)
    {
        builder.ToTable("idempotency_requests", "orders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Response)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.Key).IsUnique();
    }
}
