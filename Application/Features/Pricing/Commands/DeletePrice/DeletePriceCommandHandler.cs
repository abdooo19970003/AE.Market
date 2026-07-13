using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Pricing.Specs;
using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Aggregates.Prices.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Pricing.Commands.DeletePrice;

internal sealed class DeletePriceCommandHandler(
    IRepository<Price> repo
) : IRequestHandler<DeletePriceCommand, Result>
{
    public async Task<Result> Handle(DeletePriceCommand request, CancellationToken cancellationToken)
    {
        var price = await repo.FirstOrDefaultAsync(
            new ActivePriceByVariantAndTypeSpec(request.VariantId, request.MarketplaceId, request.Type),
            cancellationToken);

        if (price is null)
            return Result.Fail(PriceErrors.NoActivePrice);

        price.Delete();

        return Result.Success();
    }
}
