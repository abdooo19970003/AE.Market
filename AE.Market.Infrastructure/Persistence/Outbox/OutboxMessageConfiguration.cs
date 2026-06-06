using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Outbox
{
    internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_messages", "outbox");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("Id");
            builder.Property(e => e.AggregateId).HasColumnName("aggregate_id");
            builder.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(500);
            builder.Property(e => e.Payload).HasColumnName("payload").HasColumnType("jsonb");
            builder.Property(e => e.OccurredOn).HasColumnName("occurred_on").HasColumnType("timestamptz");
            builder.Property(e => e.ProcessedOn).HasColumnName("processed_on").HasColumnType("timestamptz");
            builder.Property(e => e.RetryCount).HasColumnName("retry_count");
            builder.Property(e => e.Error).HasColumnName("error");
            builder.Property(e => e.DeadLetter).HasColumnName("dead_letter");

            builder.HasIndex(e => e.AggregateId);
            builder.HasIndex(e => e.OccurredOn)
                   .HasFilter("\"processed_on\" IS NULL AND \"dead_letter\" = false");

            builder.HasQueryFilter(m => m.ProcessedOn == null && !m.DeadLetter);
        }
    }
}
