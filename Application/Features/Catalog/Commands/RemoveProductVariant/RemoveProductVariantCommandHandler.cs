using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveProductVariant;

internal sealed class RemoveProductVariantCommandHandler(
    IRepository<Product> repo
) : IRequestHandler<RemoveProductVariantCommand, Result>
{
    public async Task<Result> Handle(RemoveProductVariantCommand request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdWithTrackingAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Fail(CatalogErrors.ProductNotFound);

        var variant = product.Variants.FirstOrDefault(v => v.Id == request.VariantId);
        if (variant is null)
            return Result.Fail(CatalogErrors.VariantNotFound);

        product.RemoveVariant(variant);
        repo.Update(product);
        return Result.Success();
    }
}
