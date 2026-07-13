using AE.Market.Domain.Aggregates.Cart;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Cart;

internal sealed class CartConfiguration : IEntityTypeConfiguration<Domain.Aggregates.Cart.Cart>
{
    public void Configure(EntityTypeBuilder<Domain.Aggregates.Cart.Cart> builder)
    {
        builder.ToTable("carts", "cart");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId);
        builder.Property(x => x.SessionId);
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .HasDefaultValue(CartStatus.Active);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.SessionId).IsUnique();

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
