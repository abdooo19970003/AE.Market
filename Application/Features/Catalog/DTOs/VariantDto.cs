namespace AE.Market.Application.Features.Catalog.DTOs;

public sealed record VariantDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
}
