using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Catalog.Products.Specs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;

namespace AE.Market.Application.Features.Catalog.Services;

internal sealed class ProductVariantLookup(
    IReadRepository<ProductVariant> variantRepo,
    IReadRepository<Product> productRepo
) : IProductVariantLookup
{
    public async Task<IReadOnlyList<VariantOrderInfo>> GetOrderInfoAsync(
        IEnumerable<Guid> variantIds, CancellationToken ct = default)
    {
        var ids = variantIds.ToList();

        var variants = await variantRepo.ListWithSpecAsync(new VariantsByIdsSpec(ids), ct);
        var variantMap = variants.ToDictionary(v => v.Id);

        var productIds = variantMap.Values.Select(v => v.ProductId).Distinct().ToList();
        var products = await productRepo.ListWithSpecAsync(new ProductsByIdsSpec(productIds), ct);
        var productMap = products.ToDictionary(p => p.Id);

        var result = new List<VariantOrderInfo>(ids.Count);
        foreach (var id in ids)
        {
            if (!variantMap.TryGetValue(id, out var variant))
                continue;

            productMap.TryGetValue(variant.ProductId, out var product);

            result.Add(new VariantOrderInfo(
                variant.Id,
                variant.ProductId,
                product?.Name ?? string.Empty,
                variant.Name,
                variant.Sku.ToString()));
        }

        return result;
    }
}
