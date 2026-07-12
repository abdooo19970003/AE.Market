using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveProductRelation;

internal sealed class RemoveProductRelationCommandHandler(
    IRepository<Product> repo
) : IRequestHandler<RemoveProductRelationCommand, Result>
{
    public async Task<Result> Handle(RemoveProductRelationCommand request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdWithTrackingAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Fail(CatalogErrors.ProductNotFound);

        var relation = product.Relations.FirstOrDefault(r => r.Id == request.RelationId);
        if (relation is null)
            return Result.Fail(CatalogErrors.ProductRelationNotFound);

        product.RemoveRelation(relation);

        return Result.Success();
    }
}
