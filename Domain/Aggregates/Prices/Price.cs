using AE.Market.Domain.Aggregates.Prices.Errors;
using AE.Market.Domain.Aggregates.Prices.Events;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.ValueObjects;
using AE.Market.Domain.Exceptions;

namespace AE.Market.Domain.Aggregates.Prices;

public sealed class Price : BaseEntity, IAggregateRoot
{
    public Guid VariantId { get; private set; }
    public PriceType Type { get; private set; }
    public Money PriceAmount { get; private set; } = default!;
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }

    private Price() { }

    private Price(
        Guid id,
        Guid variantId,
        PriceType type,
        Money priceAmount,
        DateTime? validFrom,
        DateTime? validTo
    )
        : base(id)
    {
        VariantId = variantId;
        Type = type;
        PriceAmount = priceAmount;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public static Price Create(
        Guid id,
        Guid variantId,
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
            throw new ArgumentException($"ValidTo cannot be before ValidFrom or in in the past");
        var price = new Price(id, variantId, type, priceAmount, validFrom, validTo);
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
}
