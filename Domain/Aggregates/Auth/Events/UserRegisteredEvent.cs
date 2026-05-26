using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth.Events
{
    public record UserRegisteredEvent(Guid UserId) : IDomainEvent;
}
