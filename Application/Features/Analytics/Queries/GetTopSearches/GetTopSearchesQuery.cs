using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Analytics.DTOs;

namespace AE.Market.Application.Features.Analytics.Queries.GetTopSearches;

public sealed record GetTopSearchesQuery(
    int Days = 30,
    int Top = 10
) : IBaseQuery<IReadOnlyList<TopSearchDto>>;
