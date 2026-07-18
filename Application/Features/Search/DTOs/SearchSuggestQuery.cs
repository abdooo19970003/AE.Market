namespace AE.Market.Application.Features.Search.DTOs;

public sealed record SearchSuggestQuery
{
    public string Q { get; init; } = string.Empty;
    public int Size { get; init; } = 5;
}
