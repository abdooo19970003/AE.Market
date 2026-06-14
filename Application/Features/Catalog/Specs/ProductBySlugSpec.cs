using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class ProductBySlugSpec(string slug) : BaseSpecification<Product>(p => p.Slug.Value == slug)
{
    public ProductBySlugSpec(string slug, bool includeChildren) : this(slug)
    {
        if (includeChildren)
        {
            AddInclude(p => p.Variants);
            AddInclude(p => p.Images);
            AddInclude(p => p.Tags);
            AddInclude(p => p.AttributeValues);
        }
    }
}
