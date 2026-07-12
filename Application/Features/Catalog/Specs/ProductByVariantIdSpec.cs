using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class ProductByVariantIdSpec : BaseSpecification<Product>
{
    public ProductByVariantIdSpec(Guid variantId)
        : base(p => p.Variants.Any(v => v.Id == variantId))
    {
        AddInclude(p => p.Variants);
    }
}
