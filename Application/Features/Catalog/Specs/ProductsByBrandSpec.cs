using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class ProductsByBrandSpec : BaseSpecification<Product>
{
    public ProductsByBrandSpec(Guid brandId, ProductStatus? status = null)
        : base(p => p.BrandId == brandId && (status == null || p.Status == status))
    {
    }

    public ProductsByBrandSpec(Guid brandId, int page, int pageSize, ProductStatus? status = null, string? sortBy = null, bool sortDescending = false)
        : base(p => p.BrandId == brandId && (status == null || p.Status == status))
    {
        SetPagination((page - 1) * pageSize, pageSize);

        if (!string.IsNullOrEmpty(sortBy))
        {
            var orderExpr = sortBy.ToLowerInvariant() switch
            {
                "name" => (System.Linq.Expressions.Expression<Func<Product, object>>)(p => p.Name),
                "price" => p => p.ListPrice,
                "created" => p => p.CreatedAt,
                _ => p => p.CreatedAt
            };
            SetOrderBy(orderExpr, sortDescending);
        }
    }
}
