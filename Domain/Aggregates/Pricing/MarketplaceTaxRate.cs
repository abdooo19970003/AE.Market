using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Pricing;

public sealed class MarketplaceTaxRate : BaseEntity, IAggregateRoot
{
    public Guid MarketplaceId { get; private set; }
    public Guid TaxCodeId { get; private set; }
    public decimal TaxRate { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }

    private MarketplaceTaxRate() { }

    private MarketplaceTaxRate(
        Guid id,
        Guid marketplaceId,
        Guid taxCodeId,
        decimal taxRate,
        DateTime? validFrom,
        DateTime? validTo)
        : base(id)
    {
        MarketplaceId = marketplaceId;
        TaxCodeId = taxCodeId;
        TaxRate = taxRate;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public static MarketplaceTaxRate Create(
        Guid id,
        Guid marketplaceId,
        Guid taxCodeId,
        decimal taxRate,
        DateTime? validFrom = null,
        DateTime? validTo = null)
    {
        validFrom ??= DateTime.UtcNow;
        return new MarketplaceTaxRate(id, marketplaceId, taxCodeId, taxRate, validFrom, validTo);
    }

    public void Deactivate()
    {
        ValidTo = DateTime.UtcNow;
        UpdateLastModified();
    }

    public bool IsActive()
    {
        return ValidTo is null && !IsDeleted;
    }
}
