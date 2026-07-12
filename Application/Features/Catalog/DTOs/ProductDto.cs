namespace AE.Market.Application.Features.Catalog.DTOs;

public sealed record ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public string Status { get; set; } = "Active";
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public bool AllowBackOrder { get; set; }
    public int? BackOrderLimit { get; set; }
    public Guid BrandId { get; set; }
    public string ProductType { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid TaxCodeId { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
}
