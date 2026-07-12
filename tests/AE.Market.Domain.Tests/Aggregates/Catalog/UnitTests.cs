using AE.Market.Domain.Aggregates.Catalog.Units;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog;

public sealed class UnitTests
{
    public sealed class Create
    {
        [Fact]
        public void Create_WithValidData_ReturnsUnit()
        {
            var id = Guid.NewGuid();
            var groupUnitId = Guid.NewGuid();

            var unit = Unit.Create(id, "Kilogram", "kg", groupUnitId, isBaseUnit: true, exchangeRateToBaseUnit: 1m);

            unit.Id.Should().Be(id);
            unit.UnitName.Should().Be("Kilogram");
            unit.Abbreviation.Should().Be("kg");
            unit.GroupUnitId.Should().Be(groupUnitId);
            unit.IsBaseUnit.Should().BeTrue();
            unit.ExchangeRateToBaseUnit.Should().Be(1m);
            unit.FormulaType.Should().Be(FormulaType.None);
        }

        [Fact]
        public void Create_ZeroExchangeRate_Throws()
        {
            var act = () => Unit.Create(Guid.NewGuid(), "Gram", "g", Guid.NewGuid(), exchangeRateToBaseUnit: 0m);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Create_NegativeExchangeRate_Throws()
        {
            var act = () => Unit.Create(Guid.NewGuid(), "Gram", "g", Guid.NewGuid(), exchangeRateToBaseUnit: -1m);

            act.Should().Throw<DomainException>();
        }
    }

    public sealed class UpdateExchangeRate
    {
        [Fact]
        public void UpdateExchangeRate_ValidRate_UpdatesProperty()
        {
            var unit = Unit.Create(Guid.NewGuid(), "Gram", "g", Guid.NewGuid(), exchangeRateToBaseUnit: 0.001m);

            unit.UpdateExchangeRate(0.002m);

            unit.ExchangeRateToBaseUnit.Should().Be(0.002m);
        }

        [Fact]
        public void UpdateExchangeRate_Zero_Throws()
        {
            var unit = Unit.Create(Guid.NewGuid(), "Gram", "g", Guid.NewGuid(), exchangeRateToBaseUnit: 0.001m);

            var act = () => unit.UpdateExchangeRate(0m);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void UpdateExchangeRate_Negative_Throws()
        {
            var unit = Unit.Create(Guid.NewGuid(), "Gram", "g", Guid.NewGuid(), exchangeRateToBaseUnit: 0.001m);

            var act = () => unit.UpdateExchangeRate(-5m);

            act.Should().Throw<DomainException>();
        }
    }

    public sealed class SetAsBaseUnit
    {
        [Fact]
        public void SetAsBaseUnit_SetsIsBaseUnitTrueAndRateTo1()
        {
            var unit = Unit.Create(Guid.NewGuid(), "Gram", "g", Guid.NewGuid(), exchangeRateToBaseUnit: 0.001m);

            unit.SetAsBaseUnit();

            unit.IsBaseUnit.Should().BeTrue();
            unit.ExchangeRateToBaseUnit.Should().Be(1m);
            unit.FormulaType.Should().Be(FormulaType.None);
            unit.ConversionFormulaDescription.Should().BeNull();
        }
    }
}
