using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Pricing.DTOs;
using AE.Market.Application.Features.Pricing.Specs;
using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Pricing.Queries.GetPriceHistory;

internal sealed class GetPriceHistoryQueryHandler(
    IReadRepository<PriceHistory> repo,
    IMapper mapper
) : IRequestHandler<GetPriceHistoryQuery, Result<List<PriceHistoryDto>>>
{
    public async Task<Result<List<PriceHistoryDto>>> Handle(GetPriceHistoryQuery request, CancellationToken cancellationToken)
    {
        var spec = new PriceHistoryByVariantPagedSpec(request.VariantId, request.Page, request.PageSize);
        var history = await repo.ListWithSpecAsync(spec, cancellationToken);

        var dtos = history.Select(mapper.Map<PriceHistoryDto>).ToList();
        return Result<List<PriceHistoryDto>>.Success(dtos);
    }
}
