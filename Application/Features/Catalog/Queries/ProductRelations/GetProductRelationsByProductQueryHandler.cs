using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.ProductRelations;

internal sealed class GetProductRelationsByProductQueryHandler(
    IReadRepository<ProductRelation> repo,
    IReadRepository<Product> productRepo,
    IMapper mapper
) : IRequestHandler<GetProductRelationsByProductQuery, Result<List<ProductRelationDto>>>
{
    public async Task<Result<List<ProductRelationDto>>> Handle(GetProductRelationsByProductQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<List<ProductRelationDto>>.Fail(CatalogErrors.ProductNotFound);

        var spec = new ProductRelationsByProductSpec(request.ProductId);
        var relations = await repo.ListWithSpecAsync(spec, cancellationToken);
        var dtos = mapper.Map<List<ProductRelationDto>>(relations);

        return Result<List<ProductRelationDto>>.Success(dtos);
    }
}
