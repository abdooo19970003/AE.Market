using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Analytics.Commands.RecordSearchQuery;

public sealed record RecordSearchQueryCommand(
    string SearchText,
    string? Filters,
    int ResultCount,
    long LatencyMs,
    string? UserId
) : ICommand;
