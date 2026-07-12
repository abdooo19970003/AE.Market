using System.Linq.Expressions;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class ProductBySlugSpec : BaseSpecification<Product>
{
    private ProductBySlugSpec(Expression<Func<Product, bool>> criteria) : base(criteria) { }

    public static ProductBySlugSpec Create(string slug, bool includeChildren = false)
    {
        var slugObj = Slug.Create(slug);
        var spec = new ProductBySlugSpec(p => p.Slug == slugObj);
        if (includeChildren)
        {
            spec.AddInclude(p => p.Variants);
            spec.AddInclude(p => p.Images);
            spec.AddInclude(p => p.Tags);
            spec.AddInclude(p => p.AttributeValues);
        }
        return spec;
    }
}
