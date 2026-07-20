namespace AE.Market.Application.Features.Analytics.DTOs;

public sealed record TopSearchDto(string SearchText, int Count, double AverageLatencyMs, DateTime LastSearchedAt);
