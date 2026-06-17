using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Catalog;

internal sealed class AttributeGroupConfiguration : IEntityTypeConfiguration<AttributeGroup>
{
    public void Configure(EntityTypeBuilder<AttributeGroup> builder)
    {
        builder.ToTable("attribute_groups", "catalog");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GroupName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug)
            .HasConversion(v => v!.Value, v => Slug.Create(v))
            .HasMaxLength(300);
        builder.Property(x => x.SortOrder).HasDefaultValue(0);
    }
}
