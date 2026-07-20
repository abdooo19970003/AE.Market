using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Analytics.DTOs;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Analytics.Queries.GetTopSearches;

internal sealed class GetTopSearchesQueryHandler(
    IAnalyticsReadRepository analyticsRepo
) : IRequestHandler<GetTopSearchesQuery, Result<IReadOnlyList<TopSearchDto>>>
{
    public async Task<Result<IReadOnlyList<TopSearchDto>>> Handle(GetTopSearchesQuery request, CancellationToken cancellationToken)
    {
        var topSearches = await analyticsRepo.GetTopSearchesAsync(request.Days, request.Top, cancellationToken);
        return Result<IReadOnlyList<TopSearchDto>>.Success(topSearches);
    }
}
