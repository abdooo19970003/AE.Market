using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Domain.Aggregates.Orders;

public sealed class IdempotencyByKeySpec : BaseSpecification<IdempotencyRequest>
{
    public IdempotencyByKeySpec(string key)
        : base(r => r.Key == key)
    {
    }
}
