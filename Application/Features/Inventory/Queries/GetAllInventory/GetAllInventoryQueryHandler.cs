using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Inventory.DTOs;
using AE.Market.Application.Features.Inventory.Specs;
using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Queries.GetAllInventory;

internal sealed class GetAllInventoryQueryHandler(
    IReadRepository<InventoryItem> repo,
    IMapper mapper
) : IRequestHandler<GetAllInventoryQuery, Result<List<InventoryItemDto>>>
{
    public async Task<Result<List<InventoryItemDto>>> Handle(
        GetAllInventoryQuery request,
        CancellationToken cancellationToken)
    {
        var spec = new InventoryItemsListSpec(request.Page, request.PageSize);
        var items = await repo.ListWithSpecAsync(spec, cancellationToken);
        var dto = mapper.Map<List<InventoryItemDto>>(items);
        return Result<List<InventoryItemDto>>.Success(dto);
    }
}
