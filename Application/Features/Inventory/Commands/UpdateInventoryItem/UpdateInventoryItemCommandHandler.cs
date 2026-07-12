using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Inventory.Specs;
using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Aggregates.Inventory.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Commands.UpdateInventoryItem;

internal sealed class UpdateInventoryItemCommandHandler(
    IRepository<InventoryItem> repo
) : IRequestHandler<UpdateInventoryItemCommand, Result>
{
    public async Task<Result> Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await repo.GetBySpecWithTrackingAsync(
            new InventoryItemByVariantSpec(request.VariantId),
            cancellationToken);

        if (item is null)
            return Result.Fail(InventoryErrors.InventoryItemNotFound);

        if (request.LowStockThreshold.HasValue)
            item.SetLowStockThreshold(request.LowStockThreshold.Value);

        if (request.AllowBackorder.HasValue)
            item.AllowBackorderSettings(request.AllowBackorder.Value, request.BackorderLimit);

        if (request.ShippingDimensions is { } dims)
            item.SetShippingDimensions(
                Domain.Aggregates.Inventory.ShippingDimensions.Create(
                    dims.WeightInGrams,
                    dims.LongInCentimeter,
                    dims.HeightInCentimeter,
                    dims.WidthInCentimeter));

        return Result.Success();
    }
}
