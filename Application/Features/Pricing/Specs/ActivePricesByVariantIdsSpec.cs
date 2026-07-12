using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Pricing.Specs;

public sealed class ActivePricesByVariantIdsSpec : BaseSpecification<Price>
{
    public ActivePricesByVariantIdsSpec(IReadOnlyList<Guid> variantIds)
        : base(p => variantIds.Contains(p.VariantId) && p.ValidTo == null && !p.IsDeleted)
    {
    }
}
