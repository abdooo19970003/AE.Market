using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Common.ValueObjects;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Prices;

public sealed class PriceHistoryTests
{
    [Fact]
    public void Create_WithValidData_ReturnsPriceHistory()
    {
        var priceId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var oldAmount = Money.Create(10m, Currency.USD);
        var newAmount = Money.Create(15m, Currency.USD);

        var history = PriceHistory.Create(
            Guid.NewGuid(),
            priceId,
            variantId,
            oldAmount,
            newAmount,
            PriceChangeReason.ManualUpdate);

        history.PriceId.Should().Be(priceId);
        history.VariantId.Should().Be(variantId);
        history.OldAmount.Should().Be(oldAmount);
        history.NewAmount.Should().Be(newAmount);
        history.Reason.Should().Be(PriceChangeReason.ManualUpdate);
        history.ChangedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        history.ChangedBy.Should().BeNull();
    }

    [Fact]
    public void Create_WithChangedBy_SetsChangedBy()
    {
        var changedBy = Guid.NewGuid();

        var history = PriceHistory.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Money.Zero(Currency.USD),
            Money.Create(5m, Currency.USD),
            PriceChangeReason.Initial,
            changedBy);

        history.ChangedBy.Should().Be(changedBy);
    }

    [Fact]
    public void Create_ForInitialPrice_HasZeroOldAmount()
    {
        var history = PriceHistory.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Money.Zero(Currency.USD),
            Money.Create(25m, Currency.USD),
            PriceChangeReason.Initial);

        history.OldAmount.Amount.Should().Be(0m);
        history.NewAmount.Amount.Should().Be(25m);
    }
}
