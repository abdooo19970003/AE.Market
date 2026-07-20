using AE.Market.Domain.Aggregates.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Analytics;

internal sealed class SearchAnalyticsConfiguration : IEntityTypeConfiguration<SearchAnalytics>
{
    public void Configure(EntityTypeBuilder<SearchAnalytics> builder)
    {
        builder.ToTable("search_analytics", "analytics");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SearchText).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Filters).HasMaxLength(2000);
        builder.Property(x => x.ResultCount).HasDefaultValue(0);
        builder.Property(x => x.LatencyMs).HasDefaultValue(0);
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.Property(x => x.SearchedAt).IsRequired();

        builder.HasIndex(x => x.SearchedAt);
        builder.HasIndex(x => x.SearchText);
    }
}
