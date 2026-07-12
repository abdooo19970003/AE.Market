using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Inventory.DTOs;
using AE.Market.Application.Features.Inventory.Specs;
using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Queries.GetLowStockReport;

internal sealed class GetLowStockReportQueryHandler(
    IReadRepository<InventoryItem> repo
) : IRequestHandler<GetLowStockReportQuery, Result<List<LowStockReportDto>>>
{
    public async Task<Result<List<LowStockReportDto>>> Handle(GetLowStockReportQuery request, CancellationToken cancellationToken)
    {
        var spec = new LowStockReportSpec(request.Page, request.PageSize);
        var items = await repo.ListWithSpecAsync(spec, cancellationToken);

        var dtos = items.Select(i => new LowStockReportDto
        {
            VariantId = i.VariantId,
            StockQuantity = i.StockQuantity,
            ReservedQuantity = i.ReservedQuantity,
            AvailableQuantity = i.AvailableQuantity,
            LowStockThreshold = i.LowStockThreshold,
            WarehouseId = i.WarehouseId
        }).ToList();

        return Result<List<LowStockReportDto>>.Success(dtos);
    }
}
