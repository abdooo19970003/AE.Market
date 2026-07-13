using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Pricing.Specs;

public sealed class ActivePriceByVariantSpec : BaseSpecification<Price>
{
    public ActivePriceByVariantSpec(Guid variantId, Guid? marketplaceId)
        : base(p => p.VariantId == variantId && p.MarketplaceId == marketplaceId && p.ValidTo == null && !p.IsDeleted)
    {
    }
}

public sealed class ActivePriceByVariantAndTypeSpec : BaseSpecification<Price>
{
    public ActivePriceByVariantAndTypeSpec(Guid variantId, Guid? marketplaceId, PriceType type)
        : base(p => p.VariantId == variantId && p.MarketplaceId == marketplaceId && p.Type == type && p.ValidTo == null && !p.IsDeleted)
    {
    }
}

public sealed class PriceByIdSpec : BaseSpecification<Price>
{
    public PriceByIdSpec(Guid id)
        : base(p => p.Id == id && !p.IsDeleted)
    {
    }
}

public sealed class PricesByVariantSpec : BaseSpecification<Price>
{
    public PricesByVariantSpec(Guid variantId)
        : base(p => p.VariantId == variantId && !p.IsDeleted)
    {
        SetOrderBy(p => p.CreatedAt, desc: true);
    }
}

public sealed class PriceHistoryByVariantSpec : BaseSpecification<PriceHistory>
{
    public PriceHistoryByVariantSpec(Guid variantId)
        : base(p => p.VariantId == variantId)
    {
        SetOrderBy(p => p.ChangedAt, desc: true);
    }
}

public sealed class PriceHistoryByVariantPagedSpec : BaseSpecification<PriceHistory>
{
    public PriceHistoryByVariantPagedSpec(Guid variantId, int page, int pageSize)
        : base(p => p.VariantId == variantId)
    {
        SetOrderBy(p => p.ChangedAt, desc: true);
        SetPagination((page - 1) * pageSize, pageSize);
    }
}

public sealed class CostPriceByVariantSpec : BaseSpecification<Price>
{
    public CostPriceByVariantSpec(Guid variantId)
        : base(p => p.VariantId == variantId && p.Type == PriceType.Cost && p.ValidTo == null && !p.IsDeleted)
    {
    }
}

public sealed class SalePriceByVariantSpec : BaseSpecification<Price>
{
    public SalePriceByVariantSpec(Guid variantId)
        : base(p => p.VariantId == variantId && p.Type == PriceType.Sale && p.ValidTo == null && !p.IsDeleted)
    {
    }
}

public sealed class InactivePriceByVariantAndTypeSpec : BaseSpecification<Price>
{
    public InactivePriceByVariantAndTypeSpec(Guid variantId, Guid? marketplaceId, PriceType type)
        : base(p => p.VariantId == variantId && p.MarketplaceId == marketplaceId && p.Type == type && p.ValidTo != null && !p.IsDeleted)
    {
        SetOrderBy(p => p.CreatedAt, desc: true);
    }
}
