using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.AddProductRelation;

internal sealed class AddProductRelationCommandHandler(
    IRepository<Product> repo,
    IMapper mapper
) : IRequestHandler<AddProductRelationCommand, Result<ProductRelationDto>>
{
    public async Task<Result<ProductRelationDto>> Handle(AddProductRelationCommand request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdWithTrackingAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<ProductRelationDto>.Fail(CatalogErrors.ProductNotFound);

        var type = (RelationType)Enum.Parse(typeof(RelationType), request.Type);

        var relation = product.AddRelation(Guid.NewGuid(), request.RelatedProductId, type, request.SortOrder);

        var dto = mapper.Map<ProductRelationDto>(relation);
        return Result<ProductRelationDto>.Success(dto);
    }
}
