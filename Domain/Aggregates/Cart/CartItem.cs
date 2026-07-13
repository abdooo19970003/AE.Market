using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Cart;

public sealed class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Guid VariantId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime AddedAt { get; private set; }

    private CartItem() { }

    private CartItem(Guid id, Guid variantId, int quantity)
        : base(id)
    {
        VariantId = variantId;
        Quantity = quantity;
        AddedAt = DateTime.UtcNow;
    }

    internal static CartItem Create(Guid id, Guid variantId, int quantity)
    {
        return new CartItem(id, variantId, quantity);
    }

    internal void UpdateQuantity(int quantity)
    {
        Quantity = quantity;
        UpdateLastModified();
    }
}
