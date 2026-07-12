using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Inventory.Specs;
using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Aggregates.Inventory.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Commands.SetQuantity;

internal sealed class SetQuantityCommandHandler(
    IRepository<InventoryItem> repo
) : IRequestHandler<SetQuantityCommand, Result>
{
    public async Task<Result> Handle(SetQuantityCommand request, CancellationToken cancellationToken)
    {
        var item = await repo.GetBySpecWithTrackingAsync(
            new InventoryItemByVariantSpec(request.VariantId),
            cancellationToken);

        if (item is null)
            return Result.Fail(InventoryErrors.InventoryItemNotFound);

        item.SetQuantity(request.Quantity);

        return Result.Success();
    }
}
