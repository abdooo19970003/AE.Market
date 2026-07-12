using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.BundleItems;

internal sealed class GetBundleItemByIdQueryHandler(
    IReadRepository<BundleItem> repo,
    IMapper mapper
) : IRequestHandler<GetBundleItemByIdQuery, Result<BundleItemDto>>
{
    public async Task<Result<BundleItemDto>> Handle(GetBundleItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await repo.FirstOrDefaultAsync(new BundleItemByIdSpec(request.Id), cancellationToken);
        if (item is null)
            return Result<BundleItemDto>.Fail(CatalogErrors.BundleItemNotFound);

        var dto = mapper.Map<BundleItemDto>(item);
        return Result<BundleItemDto>.Success(dto);
    }
}
