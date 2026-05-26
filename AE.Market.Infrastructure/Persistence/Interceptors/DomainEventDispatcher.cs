using AE.Market.Domain.Common;
using AE.Market.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AE.Market.Infrastructure.Persistence.Interceptors
{
    internal class DomainEventDispatcher : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default
        )
        {
            if (eventData.Context is not AppDbContext db)
                return base.SavingChangesAsync(eventData, result, cancellationToken);

            //foreach (var entry in db.ChangeTracker.Entries<BaseEntity>())
            //{
            //    foreach (var domainEvent in entry.Entity.DomainEvents)
            //    {
            //        db.OutboxMessages.Add(OutboxMessage.Create(domainEvent, entry.Entity.Id));
            //    }
            //}
            var domainEvents = db
                .ChangeTracker.Entries<BaseEntity>()
                .SelectMany(e => e.Entity.DomainEvents.Select(ev => (e.Entity.Id, ev)))
                .ToList();

            foreach (var (aggregateId, domainEvent) in domainEvents)
            {
                db.OutboxMessages.Add(OutboxMessage.Create(domainEvent, aggregateId));
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default
        )
        {
            if (eventData.Context is not AppDbContext db)
                return base.SavedChangesAsync(eventData, result, cancellationToken);
            foreach (var entry in db.ChangeTracker.Entries<BaseEntity>().ToList())
            {
                entry.Entity.ClearDomainEvents();
            }
            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
