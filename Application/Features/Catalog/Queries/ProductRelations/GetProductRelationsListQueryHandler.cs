using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.Specifications;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.ProductRelations;

internal sealed class GetProductRelationsListQueryHandler(
    IReadRepository<ProductRelation> repo,
    IMapper mapper
) : IRequestHandler<GetProductRelationsListQuery, Result<PaginatedList<ProductRelationDto>>>
{
    public async Task<Result<PaginatedList<ProductRelationDto>>> Handle(GetProductRelationsListQuery request, CancellationToken cancellationToken)
    {
        var spec = new BaseSpecification<ProductRelation>();
        var totalCount = await repo.CountAsync(spec, cancellationToken);
        spec.SetPagination((request.Page - 1) * request.PageSize, request.PageSize);

        var items = await repo.ListWithSpecAsync(spec, cancellationToken);
        var dtos = mapper.Map<List<ProductRelationDto>>(items);

        var result = new PaginatedList<ProductRelationDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PaginatedList<ProductRelationDto>>.Success(result);
    }
}
