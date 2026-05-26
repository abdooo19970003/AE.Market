using System.Text.Json;
using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Quartz;

namespace AE.Market.Infrastructure.Persistence.Outbox
{
    [DisallowConcurrentExecution]
    public class OutboxProcessorJob(
        AppDbContext dbContext,
        IPublisher publisher,
        ILogger<OutboxProcessorJob> logger
    ) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync(
                context.CancellationToken
            );
            try
            {
                //List<OutboxMessage> messages = await dbContext
                //                .Set<OutboxMessage>()
                //                .Where(m => m.ProcessedOn == null && !m.DeadLetter)
                //                .OrderBy(m => m.OccurredOn)
                //                .Take(20)
                //                .ToListAsync(context.CancellationToken);

                var messages = await dbContext
                    .Database.SqlQuery<OutboxMessage> (
                        $"""
                        SELECT
                            "Id" AS Id,
                            aggregate_id AS AggregateId,
                            event_type AS EventType,
                            payload AS Payload,
                            occurred_on AS OccurredOn,
                            processed_on AS ProcessedOn,
                            retry_count AS RetryCount,
                            error AS Error,
                            dead_letter AS DeadLetter
                        FROM outbox.outbox_messages
                        WHERE processed_on IS NULL AND dead_letter = false
                        ORDER BY occurred_on
                        LIMIT 20
                        FOR UPDATE SKIP LOCKED
                        """
                    ).AsTracking()
                    .ToListAsync(context.CancellationToken);

                if (!messages.Any())
                {
                    await transaction.RollbackAsync(context.CancellationToken);
                    return;
                }
                foreach (OutboxMessage message in messages)
                {
                    try
                    {
                        Type type = Type.GetType(message.EventType)!;
                        IDomainEvent? domainEvent = (IDomainEvent?)
                            JsonSerializer.Deserialize(
                                message.Payload,
                                type,
                                JsonSerializerOptions.Default
                            );
                        if (domainEvent is null)
                            continue;
                        var wrapperType = typeof(DomainEventNotification<>).MakeGenericType(
                            domainEvent.GetType()
                        );
                        var wrapper = Activator.CreateInstance(wrapperType, domainEvent);
                        await publisher.Publish(wrapper!, context.CancellationToken);
                        message.MarkProcessed();
                    }
                    catch (Exception ex)
                    {
                        message.Failed(ex.ToString());
                        if (message.RetryCount >= 10)
                        {
                            message.SetDeadLetter();
                            logger.LogError(ex.Message, ex);
                        }
                    }
                }
                await dbContext.SaveChangesAsync(context.CancellationToken);
                await transaction.CommitAsync(context.CancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(context.CancellationToken);
                throw;
            }
        }
    }
}
