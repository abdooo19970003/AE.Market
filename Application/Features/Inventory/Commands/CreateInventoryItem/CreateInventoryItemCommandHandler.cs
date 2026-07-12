using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Inventory.DTOs;
using AE.Market.Application.Features.Inventory.Specs;
using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Aggregates.Inventory.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Commands.CreateInventoryItem;

internal sealed class CreateInventoryItemCommandHandler(
    IRepository<InventoryItem> repo,
    IMapper mapper
) : IRequestHandler<CreateInventoryItemCommand, Result<InventoryItemDto>>
{
    public async Task<Result<InventoryItemDto>> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var existing = await repo.FirstOrDefaultAsync(
            new InventoryItemByVariantSpec(request.VariantId),
            cancellationToken);

        if (existing is not null)
            return Result<InventoryItemDto>.Fail(InventoryErrors.InventoryItemAlreadyExists);

        var item = InventoryItem.Create(
            Guid.NewGuid(),
            request.VariantId,
            request.WarehouseId,
            request.StockQuantity,
            request.TrackInventory,
            request.AllowBackorder,
            request.BackorderLimit,
            request.LowStockThreshold);

        await repo.AddAsync(item, cancellationToken);

        var dto = mapper.Map<InventoryItemDto>(item);
        return Result<InventoryItemDto>.Success(dto);
    }
}
