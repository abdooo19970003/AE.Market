using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Inventory.Specs;
using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Aggregates.Inventory.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Commands.ReserveStock;

internal sealed class ReserveStockCommandHandler(
    IRepository<InventoryItem> repo
) : IRequestHandler<ReserveStockCommand, Result>
{
    public async Task<Result> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        var item = await repo.GetBySpecWithTrackingAsync(
            new InventoryItemByVariantSpec(request.VariantId),
            cancellationToken);

        if (item is null)
            return Result.Fail(InventoryErrors.InventoryItemNotFound);

        try
        {
            item.ReserveStock(request.Quantity);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(new Error("Inventory.Item.ReservationExceedsAvailable", ex.Message));
        }

        return Result.Success();
    }
}
