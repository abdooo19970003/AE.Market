using AE.Market.Domain.Aggregates.Inventory.Events;
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Inventory;

public sealed class InventoryItem : BaseEntity, IAggregateRoot
{
    public Guid VariantId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public int StockQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int AvailableQuantity => StockQuantity - ReservedQuantity;
    public bool TrackInventory { get; private set; }
    public bool AllowBackorder { get; private set; }
    public int? BackorderLimit { get; private set; }
    public int LowStockThreshold { get; private set; }
    public ShippingDimensions ShippingDimensions { get; private set; }

    private InventoryItem(
        Guid id,
        Guid variantId,
        Guid warehouseId,
        int stockQuantity,
        int reservedQuantity,
        bool trackInventory,
        bool allowBackorder,
        int? backorderLimit,
        int lowStockThreshold,
        ShippingDimensions shippingDimensions
    )
        : base(id)
    {
        VariantId = variantId;
        StockQuantity = stockQuantity;
        ReservedQuantity = reservedQuantity;
        TrackInventory = trackInventory;
        AllowBackorder = allowBackorder;
        BackorderLimit = backorderLimit;
        LowStockThreshold = lowStockThreshold;
        ShippingDimensions = shippingDimensions;
        WarehouseId = warehouseId;
    }

    private InventoryItem() { }

    public static InventoryItem Create(
        Guid id,
        Guid variantId,
        Guid warehouseId,
        int stockQuantity = 0,
        bool trackInventory = true,
        bool allowBackorder = false,
        int? backorderLimit = null,
        int lowStockThreshold = 0,
        ShippingDimensions? shippingDimensions = null
    )
    {
        if (stockQuantity < 0)
            throw new ArgumentOutOfRangeException(
                nameof(stockQuantity),
                "Stock quantity cannot be negative."
            );

        var item = new InventoryItem(
            id,
            variantId,
            warehouseId,
            stockQuantity,
            0,
            trackInventory,
            allowBackorder,
            backorderLimit,
            lowStockThreshold,
            shippingDimensions
        );

        item.AddDomainEvent(new InventoryCreatedDomainEvent(id, variantId));

        return item;
    }

    public void SetQuantity(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Stock quantity cannot be negative."
            );

        var oldQuantity = StockQuantity;
        StockQuantity = quantity;
        UpdateLastModified();

        if (oldQuantity != quantity)
            AddDomainEvent(
                new StockAdjustedDomainEvent(
                    Id,
                    VariantId,
                    oldQuantity,
                    quantity,
                    quantity - oldQuantity
                )
            );

        CheckLowStock();
    }

    public void AdjustStock(int delta)
    {
        var newQuantity = StockQuantity + delta;
        if (newQuantity < 0)
            throw new InvalidOperationException(
                $"Insufficient stock. Current: {StockQuantity}, attempted delta: {delta}."
            );

        var oldQuantity = StockQuantity;
        StockQuantity = newQuantity;
        UpdateLastModified();

        AddDomainEvent(
            new StockAdjustedDomainEvent(Id, VariantId, oldQuantity, newQuantity, delta)
        );

        CheckLowStock();
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Reservation quantity must be positive."
            );
        if (quantity > AvailableQuantity)
            throw new InvalidOperationException(
                $"Insufficient available stock. Available: {AvailableQuantity}, requested: {quantity}."
            );

        ReservedQuantity += quantity;
        if (AvailableQuantity <= 0) AddDomainEvent(new OutOfStockDomainEvent(Id, VariantId));
        UpdateLastModified();

        AddDomainEvent(new StockReservedDomainEvent(Id, VariantId, quantity, ReservedQuantity));
    }

    public void ReleaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Release quantity must be positive."
            );
        if (quantity > ReservedQuantity)
            throw new InvalidOperationException(
                $"Cannot release more than reserved quantity. Reserved: {ReservedQuantity}, requested: {quantity}."
            );

        ReservedQuantity -= quantity;
        UpdateLastModified();

        AddDomainEvent(new StockReleasedDomainEvent(Id, VariantId, quantity, ReservedQuantity));
    }

    public void SetLowStockThreshold(int threshold)
    {
        if (threshold < 0)
            throw new ArgumentOutOfRangeException(
                nameof(threshold),
                "Low stock threshold cannot be negative."
            );

        LowStockThreshold = threshold;
        UpdateLastModified();

        CheckLowStock();
    }

    public void SetShippingDimensions(ShippingDimensions dimensions)
    {
        ShippingDimensions = dimensions;
        UpdateLastModified();
    }

    public void AllowBackorderSettings(bool allow, int? limit)
    {
        AllowBackorder = allow;
        BackorderLimit = allow ? limit : null;
        UpdateLastModified();
    }

    public override void Delete() => base.Delete();

    private void CheckLowStock()
    {
        if (LowStockThreshold > 0 && StockQuantity <= LowStockThreshold)
        {
            AddDomainEvent(
                new LowStockThresholdReachedDomainEvent(
                    Id,
                    VariantId,
                    StockQuantity,
                    LowStockThreshold
                )
            );
        }
    }
}
