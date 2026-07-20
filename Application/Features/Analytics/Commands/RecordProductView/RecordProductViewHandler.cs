using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Analytics.Commands.RecordProductView;

internal sealed class RecordProductViewHandler(
    IRepository<Product> repo
) : IRequestHandler<RecordProductViewCommand, Result>
{
    public async Task<Result> Handle(RecordProductViewCommand request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Fail(new Error("Application.NotFound", "Product not found"));

        product.IncrementViewCount();

        return Result.Success();
    }
}
