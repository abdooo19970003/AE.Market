using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Inventory.Specs;
using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Aggregates.Inventory.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Commands.DeleteInventoryItem;

internal sealed class DeleteInventoryItemCommandHandler(
    IRepository<InventoryItem> repo
) : IRequestHandler<DeleteInventoryItemCommand, Result>
{
    public async Task<Result> Handle(DeleteInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await repo.FirstOrDefaultAsync(
            new InventoryItemByVariantSpec(request.VariantId),
            cancellationToken);

        if (item is null)
            return Result.Fail(InventoryErrors.InventoryItemNotFound);

        item.Delete();

        return Result.Success();
    }
}
