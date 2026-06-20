using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.ProductRelations;

internal sealed class GetProductRelationByIdQueryHandler(
    IReadRepository<ProductRelation> repo,
    IMapper mapper
) : IRequestHandler<GetProductRelationByIdQuery, Result<ProductRelationDto>>
{
    public async Task<Result<ProductRelationDto>> Handle(GetProductRelationByIdQuery request, CancellationToken cancellationToken)
    {
        var relation = await repo.FirstOrDefaultAsync(new ProductRelationByIdSpec(request.Id), cancellationToken);
        if (relation is null)
            return Result<ProductRelationDto>.Fail(CatalogErrors.ProductRelationNotFound);

        var dto = mapper.Map<ProductRelationDto>(relation);
        return Result<ProductRelationDto>.Success(dto);
    }
}
