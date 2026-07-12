using AE.Market.Domain.Aggregates.Catalog.Units;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Catalog;

internal sealed class GroupUnitConfiguration : IEntityTypeConfiguration<GroupUnit>
{
    public void Configure(EntityTypeBuilder<GroupUnit> builder)
    {
        builder.ToTable("group_units", "catalog");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GroupUnitName).HasMaxLength(200).IsRequired();

        builder.HasMany(x => x.Units)
            .WithOne()
            .HasForeignKey(x => x.GroupUnitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
