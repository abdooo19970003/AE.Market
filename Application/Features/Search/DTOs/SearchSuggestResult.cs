namespace AE.Market.Application.Features.Search.DTOs;

public sealed class SearchSuggestResult
{
    public List<SuggestionItem> Suggestions { get; set; } = [];
}

public sealed class SuggestionItem
{
    public string Text { get; set; } = string.Empty;
    public float Score { get; set; }
}
