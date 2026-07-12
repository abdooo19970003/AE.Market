using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Aggregates.Prices.Events;
using AE.Market.Domain.Common.ValueObjects;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Prices;

public sealed class PriceTests
{
    private static readonly Guid VariantId = Guid.NewGuid();
    private static readonly Guid MarketplaceId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ReturnsPrice()
    {
        var money = Money.Create(29.99m, Currency.USD);

        var price = Price.Create(Guid.NewGuid(), VariantId, MarketplaceId, PriceType.Sale, money);

        price.VariantId.Should().Be(VariantId);
        price.MarketplaceId.Should().Be(MarketplaceId);
        price.Type.Should().Be(PriceType.Sale);
        price.PriceAmount.Should().Be(money);
        price.ValidFrom.Should().NotBeNull();
        price.ValidTo.Should().BeNull();
        price.DomainEvents.Should().ContainSingle(e => e is PriceCreatedDomainEvent);
    }

    [Fact]
    public void Create_WithNullMarketplaceId_ReturnsPriceWithNullMarketplaceId()
    {
        var money = Money.Create(29.99m, Currency.USD);

        var price = Price.Create(Guid.NewGuid(), VariantId, null, PriceType.Sale, money);

        price.MarketplaceId.Should().BeNull();
    }

    [Fact]
    public void Create_WithZeroAmount_ThrowsDomainException()
    {
        var money = Money.Zero(Currency.USD);

        var act = () => Price.Create(Guid.NewGuid(), VariantId, MarketplaceId, PriceType.Sale, money);

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Prices.Price.PriceMustBePositive");
    }

    [Fact]
    public void Create_WithNegativeAmount_ThrowsDomainException()
    {
        var money = Money.Create(-5m, Currency.USD);

        var act = () => Price.Create(Guid.NewGuid(), VariantId, MarketplaceId, PriceType.Sale, money);

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Prices.Price.PriceMustBePositive");
    }

    [Fact]
    public void Create_WithValidToBeforeValidFrom_ThrowsArgumentException()
    {
        var money = Money.Create(10m, Currency.USD);
        var validFrom = DateTime.UtcNow.AddDays(5);
        var validTo = DateTime.UtcNow.AddDays(1);

        var act = () => Price.Create(Guid.NewGuid(), VariantId, MarketplaceId, PriceType.Sale, money, validFrom, validTo);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdatePrice_UpdatesAmountAndDates()
    {
        var price = Price.Create(Guid.NewGuid(), VariantId, MarketplaceId, PriceType.Sale, Money.Create(10m, Currency.USD));
        var newMoney = Money.Create(15m, Currency.USD);

        price.UpdatePrice(newMoney, null, null);

        price.PriceAmount.Should().Be(newMoney);
        price.DomainEvents.Should().ContainSingle(e => e is PriceUpdatedDomainEvent);
    }

    [Fact]
    public void Deactivate_SetsValidToAndReturnsOldAmount()
    {
        var price = Price.Create(Guid.NewGuid(), VariantId, MarketplaceId, PriceType.Sale, Money.Create(20m, Currency.USD));

        var oldAmount = price.Deactivate();

        oldAmount.Should().Be(Money.Create(20m, Currency.USD));
        price.ValidTo.Should().NotBeNull();
        price.IsActive().Should().BeFalse();
    }

    [Fact]
    public void Activate_SetsValidToNull()
    {
        var price = Price.Create(Guid.NewGuid(), VariantId, MarketplaceId, PriceType.Sale, Money.Create(20m, Currency.USD));
        price.Deactivate();

        price.Activate();

        price.ValidTo.Should().BeNull();
        price.IsActive().Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithNoDates_ReturnsTrue()
    {
        var price = Price.Create(Guid.NewGuid(), VariantId, MarketplaceId, PriceType.Sale, Money.Create(10m, Currency.USD));

        price.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsActive_WithValidToSet_ReturnsFalse()
    {
        var price = Price.Create(Guid.NewGuid(), VariantId, MarketplaceId, PriceType.Sale, Money.Create(10m, Currency.USD));
        price.Deactivate();

        price.IsActive().Should().BeFalse();
    }

    [Fact]
    public void Delete_FiresDeletedEvent()
    {
        var price = Price.Create(Guid.NewGuid(), VariantId, MarketplaceId, PriceType.Sale, Money.Create(10m, Currency.USD));

        price.Delete();

        price.DomainEvents.Should().ContainSingle(e => e is PriceDeletedDomainEvent);
        price.IsDeleted.Should().BeTrue();
    }
}
