using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth.Events
{
    public record UserProfileUpdatedEvent(Guid Id) : IDomainEvent;
}
