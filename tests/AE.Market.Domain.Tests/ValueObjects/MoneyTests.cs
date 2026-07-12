using AE.Market.Domain.Common.ValueObjects;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_WithValidAmountAndCurrency_ReturnsMoney()
    {
        var money = Money.Create(10.50m, Currency.USD);

        money.Amount.Should().Be(10.50m);
        money.Currency.Should().Be(Currency.USD);
    }

    [Fact]
    public void Zero_ReturnsZeroAmountWithCurrency()
    {
        var money = Money.Zero(Currency.EUR);

        money.Amount.Should().Be(0m);
        money.Currency.Should().Be(Currency.EUR);
    }

    [Fact]
    public void Round_RoundsToCurrencyDecimalPlaces()
    {
        var money = Money.Create(10.556m, Currency.USD);

        var rounded = money.Round();

        rounded.Amount.Should().Be(10.56m);
        rounded.Currency.Should().Be(Currency.USD);
    }

    [Fact]
    public void Round_JPY_HasZeroDecimalPlaces()
    {
        var money = Money.Create(100.6m, Currency.FromCode("JPY"));

        var rounded = money.Round();

        rounded.Amount.Should().Be(101m);
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        var a = Money.Create(10m, Currency.USD);
        var b = Money.Create(5m, Currency.USD);

        var result = a + b;

        result.Amount.Should().Be(15m);
        result.Currency.Should().Be(Currency.USD);
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsDifference()
    {
        var a = Money.Create(10m, Currency.USD);
        var b = Money.Create(3m, Currency.USD);

        var result = a - b;

        result.Amount.Should().Be(7m);
    }

    [Fact]
    public void Multiply_MoneyByDecimal_ReturnsProduct()
    {
        var money = Money.Create(10m, Currency.USD);

        var result = money * 2m;

        result.Amount.Should().Be(20m);
        result.Currency.Should().Be(Currency.USD);
    }

    [Fact]
    public void Multiply_DecimalByMoney_ReturnsProduct()
    {
        var money = Money.Create(10m, Currency.USD);

        var result = 3m * money;

        result.Amount.Should().Be(30m);
    }

    [Fact]
    public void Divide_MoneyByDecimal_ReturnsQuotient()
    {
        var money = Money.Create(10m, Currency.USD);

        var result = money / 2m;

        result.Amount.Should().Be(5m);
        result.Currency.Should().Be(Currency.USD);
    }

    [Fact]
    public void Divide_ByZero_ThrowsDivideByZeroException()
    {
        var money = Money.Create(10m, Currency.USD);

        var act = () => money / 0m;

        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Add_DifferentCurrencies_ThrowsDomainException()
    {
        var a = Money.Create(10m, Currency.USD);
        var b = Money.Create(5m, Currency.EUR);

        var act = () => a + b;

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Money.MismatchedCurrencies");
    }

    [Fact]
    public void Subtract_DifferentCurrencies_ThrowsDomainException()
    {
        var a = Money.Create(10m, Currency.USD);
        var b = Money.Create(5m, Currency.EUR);

        var act = () => a - b;

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Money.MismatchedCurrencies");
    }

    [Fact]
    public void LessThan_SameCurrency_ReturnsTrue()
    {
        var a = Money.Create(5m, Currency.USD);
        var b = Money.Create(10m, Currency.USD);

        (a < b).Should().BeTrue();
    }

    [Fact]
    public void GreaterThan_SameCurrency_ReturnsTrue()
    {
        var a = Money.Create(10m, Currency.USD);
        var b = Money.Create(5m, Currency.USD);

        (a > b).Should().BeTrue();
    }

    [Fact]
    public void LessThanOrEqual_SameValue_ReturnsTrue()
    {
        var a = Money.Create(10m, Currency.USD);
        var b = Money.Create(10m, Currency.USD);

        (a <= b).Should().BeTrue();
    }

    [Fact]
    public void GreaterThanOrEqual_SameValue_ReturnsTrue()
    {
        var a = Money.Create(10m, Currency.USD);
        var b = Money.Create(10m, Currency.USD);

        (a >= b).Should().BeTrue();
    }

    [Fact]
    public void LessThan_DifferentCurrency_ThrowsDomainException()
    {
        var a = Money.Create(5m, Currency.USD);
        var b = Money.Create(10m, Currency.EUR);

        var act = () => a < b;

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Money.MismatchedCurrencies");
    }

    [Fact]
    public void Convert_ValidRate_ReturnsConvertedMoney()
    {
        var money = Money.Create(100m, Currency.USD);

        var result = money.Convert(Currency.EUR, 0.92m);

        result.Amount.Should().Be(92m);
        result.Currency.Should().Be(Currency.EUR);
    }

    [Fact]
    public void Convert_SameCurrency_ReturnsSameMoney()
    {
        var money = Money.Create(100m, Currency.USD);

        var result = money.Convert(Currency.USD, 1m);

        result.Should().Be(money);
    }

    [Fact]
    public void Convert_InvalidRate_ThrowsDomainException()
    {
        var money = Money.Create(100m, Currency.USD);

        var act = () => money.Convert(Currency.EUR, 0m);

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Money.InvalidExchangeRate");
    }

    [Fact]
    public void Convert_NegativeRate_ThrowsDomainException()
    {
        var money = Money.Create(100m, Currency.USD);

        var act = () => money.Convert(Currency.EUR, -1m);

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Money.InvalidExchangeRate");
    }

    [Fact]
    public void ToString_WithUSD_FormatsCorrectly()
    {
        var money = Money.Create(1234.56m, Currency.USD);

        var result = money.ToString();

        result.Should().Be("1,234.56 USD");
    }

    [Fact]
    public void ToString_WithJPY_FormatsCorrectly()
    {
        var money = Money.Create(1234m, Currency.FromCode("JPY"));

        var result = money.ToString();

        result.Should().Be("1,234 JPY");
    }

    [Fact]
    public void CompareTo_SameCurrency_ReturnsExpectedValue()
    {
        var a = Money.Create(5m, Currency.USD);
        var b = Money.Create(10m, Currency.USD);

        a.CompareTo(b).Should().BeNegative();
        b.CompareTo(a).Should().BePositive();
        a.CompareTo(Money.Create(5m, Currency.USD)).Should().Be(0);
    }

    [Fact]
    public void CompareTo_WithExchangeRate_CompareDifferentCurrencies()
    {
        var usd = Money.Create(100m, Currency.USD);
        var eur = Money.Create(92m, Currency.EUR);

        usd.CompareTo(eur, 0.92m).Should().Be(0);
    }

    [Fact]
    public void FromDecimal_WithCurrencyCode_CreatesMoney()
    {
        var money = Money.FromDecimal(50m, "GBP");

        money.Amount.Should().Be(50m);
        money.Currency.Should().Be(Currency.GBP);
    }

    [Fact]
    public void FromDecimal_UnknownCurrency_ThrowsDomainException()
    {
        var act = () => Money.FromDecimal(50m, "XYZ");

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Money.UnknownCurrency");
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = Money.Create(10m, Currency.USD);
        var b = Money.Create(10m, Currency.USD);

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentAmounts_AreNotEqual()
    {
        var a = Money.Create(10m, Currency.USD);
        var b = Money.Create(20m, Currency.USD);

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_DifferentCurrencies_AreNotEqual()
    {
        var a = Money.Create(10m, Currency.USD);
        var b = Money.Create(10m, Currency.EUR);

        a.Should().NotBe(b);
    }
}
