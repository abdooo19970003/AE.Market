using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.Specifications;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.ProductImages;

internal sealed class GetProductImagesListQueryHandler(
    IReadRepository<ProductImage> repo,
    IMapper mapper
) : IRequestHandler<GetProductImagesListQuery, Result<PaginatedList<ProductImageDto>>>
{
    public async Task<Result<PaginatedList<ProductImageDto>>> Handle(GetProductImagesListQuery request, CancellationToken cancellationToken)
    {
        var spec = new BaseSpecification<ProductImage>();
        var totalCount = await repo.CountAsync(spec, cancellationToken);
        spec.SetPagination((request.Page - 1) * request.PageSize, request.PageSize);

        var items = await repo.ListWithSpecAsync(spec, cancellationToken);
        var dtos = mapper.Map<List<ProductImageDto>>(items);

        var result = new PaginatedList<ProductImageDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PaginatedList<ProductImageDto>>.Success(result);
    }
}
