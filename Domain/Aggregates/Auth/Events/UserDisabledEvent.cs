using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth.Events
{
    public sealed record UserDisabledDomainEvent(Guid UserId) : IDomainEvent;
}
