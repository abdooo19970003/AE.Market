using AE.Market.Domain.Aggregates.Cart;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Cart;

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items", "cart");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CartId).IsRequired();
        builder.Property(x => x.VariantId).IsRequired();
        builder.Property(x => x.Quantity).HasDefaultValue(1);
        builder.Property(x => x.AddedAt).HasDefaultValueSql("now()");

        builder.HasIndex(x => new { x.CartId, x.VariantId }).IsUnique();
    }
}
