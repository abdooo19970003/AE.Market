using System.Text.Json;
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Infrastructure.Persistence.Outbox
{
    public sealed class OutboxMessage
    {
        public Guid Id { get; init; }
        public Guid AggregateId { get; init; }
        public string EventType { get; init; }
        public string Payload { get; init; }
        public DateTime OccurredOn { get; init; }
        public DateTime? ProcessedOn { get; private set; }
        public int RetryCount { get; private set; } = 0;
        public string? Error { get; private set; }
        public bool DeadLetter { get; private set; } = false;

        // EF ctor
#pragma warning disable CS8618
        private OutboxMessage() { }
#pragma warning restore CS8618

        public static OutboxMessage Create(IDomainEvent @event,Guid aggregateId)
        {
            return new()
            {
                Id = Guid.NewGuid(),
                EventType = @event.GetType().FullName!,
                AggregateId = aggregateId,
                Payload = JsonSerializer.Serialize(@event, JsonSerializerOptions.Default),
                OccurredOn = DateTime.UtcNow,
            };
        }
        public void MarkProcessed() {
            ProcessedOn = DateTime.UtcNow;
        }
        public void Failed(string error)
        {
            Error = error;
            RetryCount++;
        }
        public void SetDeadLetter() { DeadLetter = true; }
    }
}
