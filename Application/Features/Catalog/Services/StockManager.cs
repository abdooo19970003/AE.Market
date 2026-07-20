using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;

namespace AE.Market.Application.Features.Catalog.Services;

internal sealed class StockManager(
    IRepository<ProductVariant> variantRepo
) : IStockManager
{
    public async Task ReserveStockAsync(Guid variantId, int quantity, CancellationToken ct = default)
    {
        var variant = await variantRepo.GetByIdWithTrackingAsync(variantId, ct);
        if (variant is null)
            return;

        variant.ReserveStock(quantity);
    }

    public async Task ReleaseStockAsync(Guid variantId, int quantity, CancellationToken ct = default)
    {
        var variant = await variantRepo.GetByIdWithTrackingAsync(variantId, ct);
        if (variant is null)
            return;

        variant.ReleaseStock(quantity);
    }
}
