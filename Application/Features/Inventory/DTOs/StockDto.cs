namespace AE.Market.Application.Features.Inventory.DTOs;

public sealed record StockDto
{
    public Guid VariantId { get; set; }
    public int StockQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public bool IsLowStock { get; set; }
}
