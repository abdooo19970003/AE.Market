using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Search.DTOs;

public sealed record SearchSuggestQuery : ISearchQuery<SearchSuggestResult>
{
    public string? Q { get; init; }
    public int Size { get; init; } = 10;
}

public sealed record SearchSuggestResult
{
    public List<SuggestionItem> Suggestions { get; init; } = [];
}

public sealed record SuggestionItem
{
    public string Text { get; init; } = string.Empty;
    public float Score { get; init; }
}
