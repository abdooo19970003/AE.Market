using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class ProductsByBrandSpec : BaseSpecification<Product>
{
    public ProductsByBrandSpec(Guid brandId, bool? isActive = null)
        : base(p => p.BrandId == brandId && (isActive == null || p.IsActive == isActive))
    {
    }

    public ProductsByBrandSpec(Guid brandId, int page, int pageSize, bool? isActive = null, string? sortBy = null, bool sortDescending = false)
        : base(p => p.BrandId == brandId && (isActive == null || p.IsActive == isActive))
    {
        SetPagination((page - 1) * pageSize, pageSize);

        if (!string.IsNullOrEmpty(sortBy))
        {
            var orderExpr = sortBy.ToLowerInvariant() switch
            {
                "name" => (System.Linq.Expressions.Expression<Func<Product, object>>)(p => p.Name),
                "price" => p => p.SalePrice,
                "created" => p => p.CreatedAt,
                _ => p => p.CreatedAt
            };
            SetOrderBy(orderExpr, sortDescending);
        }
    }
}
