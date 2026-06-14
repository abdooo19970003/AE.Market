using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Auth.Events
{
    public sealed record UserDisabledDomainEvent(Guid UserId) : IDomainEvent;
}
