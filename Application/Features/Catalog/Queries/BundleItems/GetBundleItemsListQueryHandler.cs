using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.Specifications;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.BundleItems;

internal sealed class GetBundleItemsListQueryHandler(
    IReadRepository<BundleItem> repo,
    IMapper mapper
) : IRequestHandler<GetBundleItemsListQuery, Result<PaginatedList<BundleItemDto>>>
{
    public async Task<Result<PaginatedList<BundleItemDto>>> Handle(GetBundleItemsListQuery request, CancellationToken cancellationToken)
    {
        var spec = new BaseSpecification<BundleItem>();
        var totalCount = await repo.CountAsync(spec, cancellationToken);
        spec.SetPagination((request.Page - 1) * request.PageSize, request.PageSize);

        var items = await repo.ListWithSpecAsync(spec, cancellationToken);
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
