using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Search.DTOs;

public sealed record SearchSuggestQuery : ISearchQuery<SearchSuggestResult>
{
    public string Q { get; init; } = string.Empty;
    public int Size { get; init; } = 5;
}
