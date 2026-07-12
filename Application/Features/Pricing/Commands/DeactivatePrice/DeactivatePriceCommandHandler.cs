using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Pricing.Specs;
using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Aggregates.Prices.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Pricing.Commands.DeactivatePrice;

internal sealed class DeactivatePriceCommandHandler(
    IRepository<Price> repo
) : IRequestHandler<DeactivatePriceCommand, Result>
{
    public async Task<Result> Handle(DeactivatePriceCommand request, CancellationToken cancellationToken)
    {
        var price = await repo.GetBySpecWithTrackingAsync(
            new ActivePriceByVariantAndTypeSpec(request.VariantId, request.Type),
            cancellationToken);

        if (price is null)
            return Result.Fail(PriceErrors.NoActivePrice);

        price.Deactivate();

        return Result.Success();
    }
}
