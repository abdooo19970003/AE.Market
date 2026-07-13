using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Products.Specs;

public sealed class VariantsByIdsSpec : BaseSpecification<ProductVariant>
{
    public VariantsByIdsSpec(IReadOnlyList<Guid> ids)
        : base(v => ids.Contains(v.Id) && !v.IsDeleted)
    {
    }
}
