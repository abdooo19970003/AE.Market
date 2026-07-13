using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Orders;

public sealed class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid VariantId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string VariantName { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public decimal SellPrice { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem() { }

    private OrderItem(Guid id, Guid orderId, Guid variantId, string productName, string variantName, string sku, decimal sellPrice, int quantity)
        : base(id)
    {
        OrderId = orderId;
        VariantId = variantId;
        ProductName = productName;
        VariantName = variantName;
        Sku = sku;
        SellPrice = sellPrice;
        Quantity = quantity;
    }

    internal static OrderItem Create(Guid id, Guid orderId, Guid variantId, string productName, string variantName, string sku, decimal sellPrice, int quantity)
    {
        return new OrderItem(id, orderId, variantId, productName, variantName, sku, sellPrice, quantity);
    }
}
