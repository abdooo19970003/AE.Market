using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Search.DTOs;

public sealed record SearchProductsQuery : ISearchQuery<SearchProductsResult>
{
    public string? Q { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? BrandId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? Status { get; init; }
    public bool? InStock { get; init; }
    public string? TagIds { get; init; }
    public string? AttributeFilters { get; init; }
    public string? Sort { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;
}

public sealed record SearchProductsResult
{
    public List<SearchProductsResultItem> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int Size { get; init; }
    public SearchFacets Facets { get; init; } = new();
}

public sealed record SearchProductsResultItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? ShortDescription { get; init; }
    public decimal ListPrice { get; init; }
    public string BrandName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public List<SearchResultImage> Images { get; init; } = [];
    public string StockStatus { get; init; } = string.Empty;
}

public sealed record SearchResultImage
{
    public string Url { get; init; } = string.Empty;
    public string? AltText { get; init; }
}

public sealed record SearchFacets
{
    public List<FacetItem> Categories { get; init; } = [];
    public List<FacetItem> Brands { get; init; } = [];
    public List<FacetItem> Tags { get; init; } = [];
    public List<PriceRangeFacet> PriceRanges { get; init; } = [];
}

public sealed record FacetItem
{
    public string Value { get; init; } = string.Empty;
    public int Count { get; init; }
}

public sealed record PriceRangeFacet
{
    public decimal From { get; init; }
    public decimal To { get; init; }
    public int Count { get; init; }
}
