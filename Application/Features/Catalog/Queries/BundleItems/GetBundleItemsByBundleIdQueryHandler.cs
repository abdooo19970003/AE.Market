using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.BundleItems;

internal sealed class GetBundleItemsByBundleIdQueryHandler(
    IReadRepository<BundleItem> repo,
    IMapper mapper
) : IRequestHandler<GetBundleItemsByBundleIdQuery, Result<PaginatedList<BundleItemDto>>>
{
    public async Task<Result<PaginatedList<BundleItemDto>>> Handle(
        GetBundleItemsByBundleIdQuery request,
        CancellationToken cancellationToken)
    {
        var countSpec = new BundleItemsByBundleSpec(request.BundleId);
        var totalCount = await repo.CountAsync(countSpec, cancellationToken);

        var listSpec = new BundleItemsByBundleSpec(request.BundleId, request.Page, request.PageSize);
        var items = await repo.ListWithSpecAsync(listSpec, cancellationToken);

        var dtos = mapper.Map<List<BundleItemDto>>(items);

        var result = new PaginatedList<BundleItemDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PaginatedList<BundleItemDto>>.Success(result);
    }
}
