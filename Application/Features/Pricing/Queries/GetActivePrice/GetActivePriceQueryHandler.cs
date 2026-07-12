using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Pricing.DTOs;
using AE.Market.Application.Features.Pricing.Specs;
using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Aggregates.Prices.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Pricing.Queries.GetActivePrice;

internal sealed class GetActivePriceQueryHandler(
    IReadRepository<Price> repo,
    IMapper mapper
) : IRequestHandler<GetActivePriceQuery, Result<PriceDto>>
{
    public async Task<Result<PriceDto>> Handle(GetActivePriceQuery request, CancellationToken cancellationToken)
    {
        var price = await repo.FirstOrDefaultAsync(
            new ActivePriceByVariantSpec(request.VariantId, request.MarketplaceId),
            cancellationToken);

        if (price is null)
            return Result<PriceDto>.Fail(PriceErrors.NoActivePrice);

        var dto = mapper.Map<PriceDto>(price);
        return Result<PriceDto>.Success(dto);
    }
}
