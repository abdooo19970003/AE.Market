using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveProductTag;

internal sealed class RemoveProductTagCommandHandler(
    IRepository<Product> repo
) : IRequestHandler<RemoveProductTagCommand, Result>
{
    public async Task<Result> Handle(RemoveProductTagCommand request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdWithTrackingAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Fail(CatalogErrors.ProductNotFound);

        product.RemoveTag(request.Slug);

        return Result.Success();
    }
}
