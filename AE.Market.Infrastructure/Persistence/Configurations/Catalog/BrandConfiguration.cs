using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Catalog;

internal sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("brands", "catalog");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ShortDescription).HasMaxLength(1000);
        builder.Property(x => x.LongDescription).HasMaxLength(4000);
        builder.Property(x => x.LogoUrl).HasMaxLength(1000);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.SortOrder).HasDefaultValue(0);
        builder.Property(x => x.MetaTitle).HasMaxLength(200);
        builder.Property(x => x.MetaDescription).HasMaxLength(500);
        builder.Property(x => x.MetaKeywords).HasMaxLength(500);

        builder.Property(x => x.Slug)
            .HasConversion(v => v.Value, v => Slug.Create(v))
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.WebsiteUrl)
            .HasConversion(v => v == null ? null : v.Value, v => v == null ? null : URL.CreateAbsolute(v))
            .HasMaxLength(2000);

        builder.HasIndex(x => x.Slug).IsUnique();
    }
}
