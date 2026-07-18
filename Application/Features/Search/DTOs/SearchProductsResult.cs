namespace AE.Market.Application.Features.Search.DTOs;

public sealed class SearchProductsResult
{
    public List<SearchProductsResultItem> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public SearchFacets Facets { get; set; } = new();
}

public sealed class SearchProductsResultItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public decimal ListPrice { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public List<SearchResultImage> Images { get; set; } = [];
    public string StockStatus { get; set; } = string.Empty;
}

public sealed class SearchResultImage
{
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
}

public sealed class SearchFacets
{
    public List<FacetItem> Categories { get; set; } = [];
    public List<FacetItem> Brands { get; set; } = [];
    public List<FacetItem> Tags { get; set; } = [];
    public List<PriceRangeFacet> PriceRanges { get; set; } = [];
}

public sealed class FacetItem
{
    public string Value { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class PriceRangeFacet
{
    public decimal From { get; set; }
    public decimal To { get; set; }
    public int Count { get; set; }
}
