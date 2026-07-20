namespace AE.Market.Application.Features.Search.DTOs;

public sealed record SearchLogEntry
{
    public string Query { get; init; } = string.Empty;
    public string Filters { get; init; } = "{}";
    public int ResultCount { get; init; }
    public int LatencyMs { get; init; }
    public Guid? UserId { get; init; }
    public DateTime Timestamp { get; init; }
}
