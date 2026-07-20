using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Analytics.DTOs;

namespace AE.Market.Application.Features.Analytics.Queries.GetAdminStats;

public sealed record GetAdminStatsQuery : IBaseQuery<AdminStatsDto>;
