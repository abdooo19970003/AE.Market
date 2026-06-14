using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.Specifications;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.Products;

internal sealed class GetProductsListQueryHandler(
    IReadRepository<Product> repo,
    IMapper mapper
) : IRequestHandler<GetProductsListQuery, Result<PaginatedList<ProductDto>>>
{
    public async Task<Result<PaginatedList<ProductDto>>> Handle(GetProductsListQuery request, CancellationToken cancellationToken)
    {
        var spec = new BaseSpecification<Product>(
            request.IsActive.HasValue ? (p => p.IsActive == request.IsActive.Value) : null
        );

        if (!string.IsNullOrEmpty(request.SortBy))
        {
            var orderExpr = request.SortBy.ToLowerInvariant() switch
            {
                "name" => (System.Linq.Expressions.Expression<Func<Product, object>>)(p => p.Name),
                "price" => p => p.SalePrice,
                "created" => p => p.CreatedAt,
                _ => p => p.CreatedAt
            };
            spec.SetOrderBy(orderExpr, request.SortDescending);
        }

        var totalCount = await repo.CountAsync(spec, cancellationToken);
        spec.SetPagination((request.Page - 1) * request.PageSize, request.PageSize);

        var products = await repo.ListWithSpecAsync(spec, cancellationToken);
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
