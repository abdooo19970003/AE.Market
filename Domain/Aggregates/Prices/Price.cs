using AE.Market.Domain.Aggregates.Prices.Errors;
using AE.Market.Domain.Aggregates.Prices.Events;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.ValueObjects;
using AE.Market.Domain.Exceptions;

namespace AE.Market.Domain.Aggregates.Prices;

public sealed class Price : BaseEntity, IAggregateRoot
{
    public Guid VariantId { get; private set; }
    public Guid MarketplaceId { get; private set; }
    public PriceType Type { get; private set; }
    public Money PriceAmount { get; private set; } = default!;
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }

    private Price() { }

    private Price(
        Guid id,
        Guid variantId,
        Guid marketplaceId,
        PriceType type,
        Money priceAmount,
        DateTime? validFrom,
        DateTime? validTo
    )
        : base(id)
    {
        VariantId = variantId;
        MarketplaceId = marketplaceId;
        Type = type;
        PriceAmount = priceAmount;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public static Price Create(
        Guid id,
        Guid variantId,
        Guid marketplaceId,
        PriceType type,
        Money priceAmount,
        DateTime? validFrom = null,
        DateTime? validTo = null
    )
    {
        if (priceAmount.Amount <= 0)
            throw new DomainException(PriceErrors.PriceMustBePositive.Code, PriceErrors.PriceMustBePositive.Message);

        validFrom ??= DateTime.UtcNow;
        if(validTo is not null &&  validFrom >= validTo)
            throw new ArgumentException($"ValidTo cannot be before ValidFrom or in the past");
        var price = new Price(id, variantId, marketplaceId, type, priceAmount, validFrom, validTo);
        price.AddDomainEvent(new PriceCreatedDomainEvent(price.Id, price.VariantId));
        return price;
    }

    public void UpdatePrice(Money newAmount, DateTime? validFrom, DateTime? validTo)
    {
        PriceAmount = newAmount;
        ValidFrom = validFrom;
        ValidTo = validTo;
        AddDomainEvent(new PriceUpdatedDomainEvent(Id, VariantId));
        UpdateLastModified();
    }

    public Money Deactivate()
    {
        var oldAmount = PriceAmount;
        ValidTo = DateTime.UtcNow;
        AddDomainEvent(new PriceUpdatedDomainEvent(Id, VariantId));
        UpdateLastModified();
        return oldAmount;
    }

    public void Activate(DateTime? newValidFrom = null)
    {
        ValidFrom = newValidFrom ?? DateTime.UtcNow;
        ValidTo = null;
        UpdateLastModified();
    }

    public override void Delete()
    {
        AddDomainEvent(new PriceDeletedDomainEvent(Id, VariantId));
        base.Delete();
    }

    public bool IsValid()
    {
        if (ValidFrom is null && ValidTo is null)
            return true;

        var now = DateTime.UtcNow;

        if (ValidFrom is not null && now < ValidFrom.Value)
            return false;

        if (ValidTo is not null && now > ValidTo.Value)
            return false;

        return true;
    }

    public bool IsActive()
    {
        return ValidTo is null && IsValid();
    }
}
