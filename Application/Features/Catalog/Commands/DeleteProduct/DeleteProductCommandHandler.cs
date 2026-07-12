using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteProduct;

internal sealed class DeleteProductCommandHandler(
    IRepository<Product> repo
) : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (product is null)
            return Result.Fail(CatalogErrors.ProductNotFound);

        repo.Delete(product);
        return Result.Success();
    }
}
