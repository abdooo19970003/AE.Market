using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Common.Abstracts;
using FluentAssertions;
using MediatR;
using NetArchTest.Rules;

namespace AE.Market.ArchitectureTests.Domain
{
    public class DomainEventHandlerTests:BaseTest
    {
        [Fact]
        public void EveryDomainEvent_ShouldHave_HandlerInApplication()
        {
            // 1. Get all domain event types from Domain assembly
            var domainEvents = Types
                .InAssembly(DomainAssembly)
                .That()
                .ImplementInterface(typeof(IDomainEvent))
                .GetTypes()
                .ToHashSet();

            // 2. Get all handler types from Application assembly
            // They implement INotificationHandler<DomainEventNotification<T>>
            var handlers = Types
                .InAssembly(ApplicationAssembly)
                .That()
                .ImplementInterface(typeof(INotificationHandler<>))
                .GetTypes();

            // 3. Extract the handled domain event type from each handler
            var handledEvents = new HashSet<Type>();
            foreach (var handler in handlers)
            {
                var iface = handler.GetInterfaces()
                    .First(i => i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>));
                var notificationType = iface.GetGenericArguments()[0]; // DomainEventNotification<T>
                if (notificationType.IsGenericType
                    && notificationType.GetGenericTypeDefinition() == typeof(DomainEventNotification<>))
                {
                    handledEvents.Add(notificationType.GetGenericArguments()[0]);
                }
            }

            // 4. Find missing handlers
            var missing = domainEvents.Except(handledEvents).ToList();
            missing.Should().BeEmpty(
                "because every domain event must have a corresponding " +
                "INotificationHandler<DomainEventNotification<T>> in the Application layer. " +
                $"Missing handlers for: [{string.Join(", ", missing.Select(t => t.Name))}]");
        }
    }
}
