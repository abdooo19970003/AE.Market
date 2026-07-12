using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Inventory.Specs;
using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Aggregates.Inventory.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Commands.ReleaseStock;

internal sealed class ReleaseStockCommandHandler(
    IRepository<InventoryItem> repo
) : IRequestHandler<ReleaseStockCommand, Result>
{
    public async Task<Result> Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
    {
        var item = await repo.GetBySpecWithTrackingAsync(
            new InventoryItemByVariantSpec(request.VariantId),
            cancellationToken);

        if (item is null)
            return Result.Fail(InventoryErrors.InventoryItemNotFound);

        try
        {
            item.ReleaseStock(request.Quantity);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(new Error("Inventory.Item.CannotReleaseMoreThanReserved", ex.Message));
        }

        return Result.Success();
    }
}
