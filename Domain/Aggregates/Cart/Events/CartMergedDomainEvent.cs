using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Cart.Events;

public sealed record CartMergedDomainEvent(Guid UserId, Guid GuestCartId, int ItemsTransferredCount) : IDomainEvent;
