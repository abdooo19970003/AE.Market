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
