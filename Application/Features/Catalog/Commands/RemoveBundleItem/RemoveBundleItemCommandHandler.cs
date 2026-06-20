using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveBundleItem;

internal sealed class RemoveBundleItemCommandHandler(
    IRepository<Product> repo
) : IRequestHandler<RemoveBundleItemCommand, Result>
{
    public async Task<Result> Handle(RemoveBundleItemCommand request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdWithTrackingAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Fail(CatalogErrors.ProductNotFound);

        product.RemoveBundleItem(request.BundleItemId);

        return Result.Success();
    }
}
