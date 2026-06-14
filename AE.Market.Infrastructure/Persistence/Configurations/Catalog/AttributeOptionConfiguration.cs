using AE.Market.Domain.Aggregates.Catalog.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Catalog;

internal sealed class AttributeOptionConfiguration : IEntityTypeConfiguration<AttributeOption>
{
    public void Configure(EntityTypeBuilder<AttributeOption> builder)
    {
        builder.ToTable("attribute_options", "catalog");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Label).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SortOrder).HasDefaultValue(0);
    }
}
