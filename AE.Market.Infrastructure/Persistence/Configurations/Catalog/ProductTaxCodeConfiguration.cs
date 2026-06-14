using AE.Market.Domain.Aggregates.Catalog.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Catalog;

internal sealed class ProductTaxCodeConfiguration : IEntityTypeConfiguration<ProductTaxCode>
{
    public void Configure(EntityTypeBuilder<ProductTaxCode> builder)
    {
        builder.ToTable("product_tax_codes", "catalog");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Type).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PerformanceLocationRequirement).HasMaxLength(500);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
