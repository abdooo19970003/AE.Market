using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Orders;

public sealed class IdempotencyRequest : BaseEntity
{
    public string Key { get; private set; } = string.Empty;
    public string Response { get; private set; } = string.Empty;
    public new DateTime CreatedAt { get; private set; }

    private IdempotencyRequest() { }

    public IdempotencyRequest(Guid id, string key, string response)
        : base(id)
    {
        Key = key;
        Response = response;
        CreatedAt = DateTime.UtcNow;
    }
}
