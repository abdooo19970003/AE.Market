using AE.Market.Domain.Common;
using MediatR;

namespace AE.Market.Application.Common.Behaviors
{
    public record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
        where TDomainEvent : IDomainEvent;
}
