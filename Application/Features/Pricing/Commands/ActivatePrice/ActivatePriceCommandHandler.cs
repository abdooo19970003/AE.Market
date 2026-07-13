using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Pricing.Specs;
using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Aggregates.Prices.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Pricing.Commands.ActivatePrice;

internal sealed class ActivatePriceCommandHandler(
    IRepository<Price> repo
) : IRequestHandler<ActivatePriceCommand, Result>
{
    public async Task<Result> Handle(ActivatePriceCommand request, CancellationToken cancellationToken)
    {
        var price = await repo.GetBySpecWithTrackingAsync(
            new InactivePriceByVariantAndTypeSpec(request.VariantId, request.MarketplaceId, request.Type),
            cancellationToken);

        if (price is null)
            return Result.Fail(PriceErrors.PriceAlreadyInactive);

        price.Activate(request.NewValidFrom);

        return Result.Success();
    }
}
