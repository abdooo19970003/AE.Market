using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Analytics.DTOs;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Analytics.Queries.GetTopProducts;

internal sealed class GetTopProductsQueryHandler(
    IAnalyticsReadRepository analyticsRepo
) : IRequestHandler<GetTopProductsQuery, Result<IReadOnlyList<TopProductDto>>>
{
    public async Task<Result<IReadOnlyList<TopProductDto>>> Handle(GetTopProductsQuery request, CancellationToken cancellationToken)
    {
        var topProducts = await analyticsRepo.GetTopProductsAsync(request.Days, request.Top, cancellationToken);
        return Result<IReadOnlyList<TopProductDto>>.Success(topProducts);
    }
}
