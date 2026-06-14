namespace AE.Market.Application.Features.Catalog.DTOs;

public sealed record ProductDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public bool IsActive { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public bool AllowBackOrder { get; set; }
    public int? BackOrderLimit { get; set; }
    public Guid BrandId { get; set; }
    public string? BrandName { get; set; }
    public string? BrandSlug { get; set; }
    public string ProductType { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategorySlug { get; set; }
    public Guid TaxCodeId { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public List<VariantDto> Variants { get; set; } = [];
    public List<string> Images { get; set; } = [];
}
