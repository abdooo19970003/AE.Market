namespace AE.Market.Application.Features.Search.DTOs;

public sealed record ProductDocument
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? ShortDescription { get; init; }
    public string? LongDescription { get; init; }
    public string? Details { get; init; }
    public string Status { get; init; } = string.Empty;
    public string ProductType { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public Guid BrandId { get; init; }
    public string BrandName { get; init; } = string.Empty;
    public List<string> Tags { get; init; } = [];
    public decimal ListPrice { get; init; }
    public int StockQuantity { get; init; }
    public bool AllowBackOrder { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? MetaKeywords { get; init; }
    public List<VariantDocument> Variants { get; init; } = [];
    public List<ImageDocument> Images { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed record VariantDocument
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal ListPrice { get; init; }
    public int StockQuantity { get; init; }
    public List<VariantAttributeValueDocument> AttributeValues { get; init; } = [];
}

public sealed record VariantAttributeValueDocument
{
    public Guid AttributeId { get; init; }
    public string AttributeName { get; init; } = string.Empty;
    public string OptionValue { get; init; } = string.Empty;
}

public sealed record ImageDocument
{
    public string Url { get; init; } = string.Empty;
    public string? AltText { get; init; }
    public int SortOrder { get; init; }
}
