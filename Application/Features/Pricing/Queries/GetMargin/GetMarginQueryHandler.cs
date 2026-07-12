using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Pricing.DTOs;
using AE.Market.Application.Features.Pricing.Specs;
using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Aggregates.Prices.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Pricing.Queries.GetMargin;

internal sealed class GetMarginQueryHandler(
    IReadRepository<Price> repo
) : IRequestHandler<GetMarginQuery, Result<MarginDto>>
{
    public async Task<Result<MarginDto>> Handle(GetMarginQuery request, CancellationToken cancellationToken)
    {
        var costPrice = await repo.FirstOrDefaultAsync(
            new CostPriceByVariantSpec(request.VariantId),
            cancellationToken);

        var salePrice = await repo.FirstOrDefaultAsync(
            new SalePriceByVariantSpec(request.VariantId),
            cancellationToken);

        if (costPrice is null || salePrice is null)
            return Result<MarginDto>.Fail(PriceErrors.NoActivePrice);

        var cost = costPrice.PriceAmount.Amount;
        var sell = salePrice.PriceAmount.Amount;

        var profit = sell - cost;
        var marginPercentage = cost > 0 ? (profit / cost) * 100m : 0m;

        var dto = new MarginDto
        {
            VariantId = request.VariantId,
            CostPrice = cost,
            SellPrice = sell,
            Currency = costPrice.PriceAmount.Currency.Code,
            MarginPercentage = Math.Round(marginPercentage, 2),
            Profit = profit
        };

        return Result<MarginDto>.Success(dto);
    }
}
