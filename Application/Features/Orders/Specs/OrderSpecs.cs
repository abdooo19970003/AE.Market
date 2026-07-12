using AE.Market.Domain.Aggregates.Orders;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Orders.Specs;

public sealed class OrderByIdSpec : BaseSpecification<Order>
{
    public OrderByIdSpec(Guid id)
        : base(o => o.Id == id && !o.IsDeleted)
    {
        AddInclude(q => q.Items);
    }
}

public sealed class OrdersByUserIdSpec : BaseSpecification<Order>
{
    public OrdersByUserIdSpec(Guid userId)
        : base(o => o.UserId == userId && !o.IsDeleted)
    {
        AddInclude(q => q.Items);
        SetOrderBy(o => o.PlacedAt, desc: true);
    }
}
