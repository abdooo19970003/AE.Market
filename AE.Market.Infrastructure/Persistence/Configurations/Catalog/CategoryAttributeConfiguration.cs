using AE.Market.Domain.Aggregates.Catalog.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Catalog;

internal sealed class CategoryAttributeConfiguration : IEntityTypeConfiguration<CategoryAttribute>
{
    public void Configure(EntityTypeBuilder<CategoryAttribute> builder)
    {
        builder.ToTable("category_attributes", "catalog");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AttributeName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(300);
        builder.Property(x => x.InputType).HasConversion<int>().IsRequired();
        builder.Property(x => x.IsRequired).HasDefaultValue(false);
        builder.Property(x => x.IsFilterable).HasDefaultValue(false);
        builder.Property(x => x.SortOrder).HasDefaultValue(0);

        builder.HasMany(x => x.Options)
            .WithOne()
            .HasForeignKey(x => x.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
