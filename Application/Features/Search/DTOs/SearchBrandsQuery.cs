using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Search.DTOs;

public sealed record SearchBrandsQuery : ISearchQuery<SearchBrandsResult>
{
    public string? Q { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;
}

public sealed record SearchBrandsResult
{
    public List<SearchBrandsResultItem> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int Size { get; init; }
}

public sealed record SearchBrandsResultItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? ShortDescription { get; init; }
    public string? LogoUrl { get; init; }
    public bool IsActive { get; init; }
    public int ProductCount { get; init; }
}
