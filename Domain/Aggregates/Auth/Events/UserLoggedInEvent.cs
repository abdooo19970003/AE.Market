using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Auth.Events
{
    public sealed record UserLoggedInDomainEvent(Guid Id) : IDomainEvent;
}
