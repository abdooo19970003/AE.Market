using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Products;

public sealed class BundleItem : BaseEntity
{
    internal BundleItem(Guid id, Guid bundleId, Guid itemId, int quantity)
        : base(id)
    {
        BundleId = bundleId;
        ItemId = itemId;
        Quantity = quantity;
    }

    private BundleItem()
    {
    }

    public Guid BundleId { get; private set; }
    public Product? Bundle { get; private set; }

    public Guid ItemId { get; private set; }
    public Product? Item { get; private set; }

    public int Quantity { get; private set; }

    internal void SetQuantity(int quantity)
    {
        Quantity = quantity;
        UpdateLastModified();
    }
}