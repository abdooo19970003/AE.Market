using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth.Events
{
    public record UserProfileCreatedEvent(Guid Id,Guid UserId) : IDomainEvent;
}
