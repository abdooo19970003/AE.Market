using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveProductImage;

internal sealed class RemoveProductImageCommandHandler(
    IRepository<Product> repo
) : IRequestHandler<RemoveProductImageCommand, Result>
{
    public async Task<Result> Handle(RemoveProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdWithTrackingAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Fail(CatalogErrors.ProductNotFound);

        var image = product.Images.FirstOrDefault(i => i.Id == request.ImageId);
        if (image is null)
            return Result.Fail(CatalogErrors.ProductNotFound);

        product.RemoveImage(image);

        return Result.Success();
    }
}
