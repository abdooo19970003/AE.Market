using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth.Events
{
    public sealed record UserProfileCreatedDomainEvent(Guid Id,Guid UserId) : IDomainEvent;
}
