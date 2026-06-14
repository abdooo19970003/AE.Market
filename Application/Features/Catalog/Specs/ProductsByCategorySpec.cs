using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class ProductsByCategorySpec : BaseSpecification<Product>
{
    public ProductsByCategorySpec(Guid categoryId, bool? isActive = null)
        : base(p => p.CategoryId == categoryId && (isActive == null || p.IsActive == isActive))
    {
    }

    public ProductsByCategorySpec(Guid categoryId, int page, int pageSize, bool? isActive = null, string? sortBy = null, bool sortDescending = false)
        : base(p => p.CategoryId == categoryId && (isActive == null || p.IsActive == isActive))
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
