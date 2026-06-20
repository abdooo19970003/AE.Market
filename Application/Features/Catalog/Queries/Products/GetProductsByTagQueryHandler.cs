using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.Products;

internal sealed class GetProductsByTagQueryHandler(
    IReadRepository<Product> repo,
    IMapper mapper
) : IRequestHandler<GetProductsByTagQuery, Result<PaginatedList<ProductDto>>>
{
    public async Task<Result<PaginatedList<ProductDto>>> Handle(
        GetProductsByTagQuery request,
        CancellationToken cancellationToken)
    {
        var countSpec = new ProductsByTagSpec(request.TagSlug, request.IsActive);
        var totalCount = await repo.CountAsync(countSpec, cancellationToken);

        var listSpec = new ProductsByTagSpec(request.TagSlug, request.Page, request.PageSize, request.IsActive);
        var products = await repo.ListWithSpecAsync(listSpec, cancellationToken);

        var dtos = mapper.Map<List<ProductDto>>(products);

        var result = new PaginatedList<ProductDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PaginatedList<ProductDto>>.Success(result);
    }
}
