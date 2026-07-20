using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Analytics.DTOs;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Analytics.Queries.GetAdminStats;

internal sealed class GetAdminStatsQueryHandler(
    IAnalyticsReadRepository analyticsRepo
) : IRequestHandler<GetAdminStatsQuery, Result<AdminStatsDto>>
{
    public async Task<Result<AdminStatsDto>> Handle(GetAdminStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await analyticsRepo.GetAdminStatsAsync(cancellationToken);
        return Result<AdminStatsDto>.Success(stats);
    }
}
