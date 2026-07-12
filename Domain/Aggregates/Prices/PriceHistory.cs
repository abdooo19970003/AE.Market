using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.ValueObjects;

namespace AE.Market.Domain.Aggregates.Prices;

public sealed class PriceHistory : BaseEntity
{
    public Guid PriceId { get; private set; }
    public Guid VariantId { get; private set; }
    public Money OldAmount { get; private set; } = default!;
    public Money NewAmount { get; private set; } = default!;
    public PriceChangeReason Reason { get; private set; }
    public Guid? ChangedBy { get; private set; }
    public DateTime ChangedAt { get; private set; }

    private PriceHistory() { }

    private PriceHistory(
        Guid id,
        Guid priceId,
        Guid variantId,
        Money oldAmount,
        Money newAmount,
        PriceChangeReason reason,
        Guid? changedBy
    )
        : base(id)
    {
        PriceId = priceId;
        VariantId = variantId;
        OldAmount = oldAmount;
        NewAmount = newAmount;
        Reason = reason;
        ChangedBy = changedBy;
        ChangedAt = DateTime.UtcNow;
    }

    public static PriceHistory Create(
        Guid id,
        Guid priceId,
        Guid variantId,
        Money oldAmount,
        Money newAmount,
        PriceChangeReason reason,
        Guid? changedBy = null
    )
    {
        return new PriceHistory(id, priceId, variantId, oldAmount, newAmount, reason, changedBy);
    }
}
