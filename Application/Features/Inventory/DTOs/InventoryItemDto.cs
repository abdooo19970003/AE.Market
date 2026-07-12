namespace AE.Market.Application.Features.Inventory.DTOs;

public sealed record InventoryItemDto
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public Guid WarehouseId { get; set; }
    public int StockQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public bool TrackInventory { get; set; }
    public bool AllowBackorder { get; set; }
    public int? BackorderLimit { get; set; }
    public int LowStockThreshold { get; set; }
}
