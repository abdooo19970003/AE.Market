using AE.Market.Domain.Aggregates.Pricing;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Pricing.Specs;

public sealed class MarketplaceByIdSpec : BaseSpecification<Marketplace>
{
    public MarketplaceByIdSpec(Guid id)
        : base(m => m.Id == id && !m.IsDeleted)
    {
    }
}

public sealed class MarketplaceByCodeSpec : BaseSpecification<Marketplace>
{
    public MarketplaceByCodeSpec(string code)
        : base(m => m.Code == code && !m.IsDeleted)
    {
    }
}

public sealed class ActiveMarketplaceTaxRateSpec : BaseSpecification<MarketplaceTaxRate>
{
    public ActiveMarketplaceTaxRateSpec(Guid marketplaceId, Guid taxCodeId)
        : base(r => r.MarketplaceId == marketplaceId
            && r.TaxCodeId == taxCodeId
            && r.ValidTo == null
            && !r.IsDeleted)
    {
    }
}

public sealed class AllMarketplacesSpec : BaseSpecification<Marketplace>
{
    public AllMarketplacesSpec()
        : base(m => !m.IsDeleted)
    {
    }
}
