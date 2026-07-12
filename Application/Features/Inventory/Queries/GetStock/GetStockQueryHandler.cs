using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Inventory.DTOs;
using AE.Market.Application.Features.Inventory.Specs;
using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Aggregates.Inventory.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Queries.GetStock;

internal sealed class GetStockQueryHandler(
    IReadRepository<InventoryItem> repo
) : IRequestHandler<GetStockQuery, Result<StockDto>>
{
    public async Task<Result<StockDto>> Handle(GetStockQuery request, CancellationToken cancellationToken)
    {
        var item = await repo.FirstOrDefaultAsync(
            new InventoryItemByVariantSpec(request.VariantId),
            cancellationToken);

        if (item is null)
            return Result<StockDto>.Fail(InventoryErrors.InventoryItemNotFound);

        var dto = new StockDto
        {
            VariantId = item.VariantId,
            StockQuantity = item.StockQuantity,
            ReservedQuantity = item.ReservedQuantity,
            AvailableQuantity = item.AvailableQuantity,
            LowStockThreshold = item.LowStockThreshold,
            IsLowStock = item.LowStockThreshold > 0 && item.StockQuantity <= item.LowStockThreshold
        };

        return Result<StockDto>.Success(dto);
    }
}
