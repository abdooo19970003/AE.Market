using AE.Market.Domain.Common.Specifications;
using CartAggregate = AE.Market.Domain.Aggregates.Cart.Cart;

namespace AE.Market.Application.Features.Cart.Specs;

public sealed class CartByUserIdSpec : BaseSpecification<CartAggregate>
{
    public CartByUserIdSpec(Guid userId)
        : base(c => c.UserId == userId && c.Status == AE.Market.Domain.Aggregates.Cart.CartStatus.Active && !c.IsDeleted)
    {
        AddInclude(q => q.Items);
    }
}

public sealed class CartBySessionIdSpec : BaseSpecification<CartAggregate>
{
    public CartBySessionIdSpec(Guid sessionId)
        : base(c => c.SessionId == sessionId && c.Status == AE.Market.Domain.Aggregates.Cart.CartStatus.Active && !c.IsDeleted)
    {
    }
}

public sealed class CartByIdSpec : BaseSpecification<CartAggregate>
{
    public CartByIdSpec(Guid id)
        : base(c => c.Id == id && !c.IsDeleted)
    {
        AddInclude(q => q.Items);
    }
}
