using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class BundleItemsByBundleSpec : BaseSpecification<BundleItem>
{
    public BundleItemsByBundleSpec(Guid bundleId)
        : base(bi => bi.BundleId == bundleId)
    {
    }

    public BundleItemsByBundleSpec(Guid bundleId, int page, int pageSize)
        : base(bi => bi.BundleId == bundleId)
    {
        SetPagination((page - 1) * pageSize, pageSize);
    }
}
