using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Aggregates.Catalog.Units;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog;

public sealed class GroupUnitTests
{
    private static GroupUnit CreateWeightGroup()
    {
        var group = GroupUnit.Create(Guid.NewGuid(), "Weight");
        group.AddUnit(Guid.NewGuid(), "Kilogram", "kg", isBaseUnit: true, exchangeRateToBaseUnit: 1m);
        group.AddUnit(Guid.NewGuid(), "Gram", "g", exchangeRateToBaseUnit: 0.001m);
        return group;
    }

    private static GroupUnit CreateTemperatureGroup()
    {
        var group = GroupUnit.Create(Guid.NewGuid(), "Temperature");
        group.AddUnit(Guid.NewGuid(), "Celsius", "°C", isBaseUnit: true,
            formulaType: FormulaType.None, conversionFormulaDescription: "Base unit");
        group.AddUnit(Guid.NewGuid(), "Fahrenheit", "°F",
            formulaType: FormulaType.FahrenheitToCelsius, conversionFormulaDescription: "F = C * 9/5 + 32");
        return group;
    }

    public sealed class Create
    {
        [Fact]
        public void Create_WithValidData_ReturnsGroupUnit()
        {
            var id = Guid.NewGuid();

            var group = GroupUnit.Create(id, "Weight");

            group.Id.Should().Be(id);
            group.GroupUnitName.Should().Be("Weight");
        }

        [Fact]
        public void Create_RaisesGroupUnitCreatedDomainEvent()
        {
            var group = GroupUnit.Create(Guid.NewGuid(), "Weight");

            group.DomainEvents.Should().ContainSingle(e => e is GroupUnitCreatedDomainEvent)
                .Which.As<GroupUnitCreatedDomainEvent>().GroupUnitId.Should().Be(group.Id);
        }
    }

    public sealed class AddUnit
    {
        [Fact]
        public void AddUnit_AddsToCollection()
        {
            var group = GroupUnit.Create(Guid.NewGuid(), "Weight");

            var unit = group.AddUnit(Guid.NewGuid(), "Kilogram", "kg", isBaseUnit: true, exchangeRateToBaseUnit: 1m);

            group.Units.Should().ContainSingle().Which.Should().Be(unit);
        }

        [Fact]
        public void AddUnit_SecondBaseUnit_Throws()
        {
            var group = GroupUnit.Create(Guid.NewGuid(), "Weight");
            group.AddUnit(Guid.NewGuid(), "Kilogram", "kg", isBaseUnit: true, exchangeRateToBaseUnit: 1m);

            var act = () => group.AddUnit(Guid.NewGuid(), "Pound", "lb", isBaseUnit: true, exchangeRateToBaseUnit: 0.453592m);

            act.Should().Throw<DomainException>();
            group.Units.Should().ContainSingle();
        }
    }

    public sealed class RemoveUnit
    {
        [Fact]
        public void RemoveUnit_RemovesAndSoftDeletes()
        {
            var group = GroupUnit.Create(Guid.NewGuid(), "Weight");
            var unit = group.AddUnit(Guid.NewGuid(), "Kilogram", "kg", isBaseUnit: true, exchangeRateToBaseUnit: 1m);

            group.RemoveUnit(unit);

            group.Units.Should().BeEmpty();
            unit.IsDeleted.Should().BeTrue();
        }
    }

    public sealed class Rename
    {
        [Fact]
        public void Rename_UpdatesName()
        {
            var group = GroupUnit.Create(Guid.NewGuid(), "Weight");

            group.Rename("Mass");

            group.GroupUnitName.Should().Be("Mass");
        }

        [Fact]
        public void Rename_RaisesGroupUnitRenamedDomainEvent()
        {
            var group = GroupUnit.Create(Guid.NewGuid(), "Weight");

            group.Rename("Mass");

            group.DomainEvents.Should().Contain(e => e is GroupUnitRenamedDomainEvent);
        }
    }

    public sealed class DeleteCascade
    {
        [Fact]
        public void Delete_CascadesToAllUnits()
        {
            var group = GroupUnit.Create(Guid.NewGuid(), "Weight");
            var unit1 = group.AddUnit(Guid.NewGuid(), "Kilogram", "kg", isBaseUnit: true, exchangeRateToBaseUnit: 1m);
            var unit2 = group.AddUnit(Guid.NewGuid(), "Gram", "g", exchangeRateToBaseUnit: 0.001m);

            group.Delete();

            group.IsDeleted.Should().BeTrue();
            unit1.IsDeleted.Should().BeTrue();
            unit2.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Delete_RaisesGroupUnitDeletedDomainEvent()
        {
            var group = GroupUnit.Create(Guid.NewGuid(), "Weight");

            group.Delete();

            group.DomainEvents.Should().Contain(e => e is GroupUnitDeletedDomainEvent);
        }
    }

    public sealed class Convert
    {
        [Fact]
        public void Convert_SameUnit_ReturnsSameValue()
        {
            var group = CreateWeightGroup();
            var kilogram = group.Units.First(u => u.Abbreviation == "kg");

            var result = group.Convert(5m, kilogram, kilogram);

            result.Should().Be(5m);
        }

        [Fact]
        public void Convert_LinearUnits_CorrectMath()
        {
            var group = CreateWeightGroup();
            var kilogram = group.Units.First(u => u.Abbreviation == "kg");
            var gram = group.Units.First(u => u.Abbreviation == "g");

            var kgToG = group.Convert(1m, kilogram, gram);

            kgToG.Should().Be(1000m);
        }

        [Fact]
        public void Convert_GramsToKilograms_CorrectMath()
        {
            var group = CreateWeightGroup();
            var kilogram = group.Units.First(u => u.Abbreviation == "kg");
            var gram = group.Units.First(u => u.Abbreviation == "g");

            var gToKg = group.Convert(500m, gram, kilogram);

            gToKg.Should().Be(0.5m);
        }

        [Fact]
        public void Convert_DifferentGroups_Throws()
        {
            var weightGroup = CreateWeightGroup();
            var tempGroup = CreateTemperatureGroup();
            var kilogram = weightGroup.Units.First(u => u.Abbreviation == "kg");
            var celsius = tempGroup.Units.First(u => u.Abbreviation == "°C");

            var act = () => weightGroup.Convert(1m, kilogram, celsius);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Convert_CelsiusToFahrenheit_CorrectMath()
        {
            var group = CreateTemperatureGroup();
            var celsius = group.Units.First(u => u.Abbreviation == "°C");
            var fahrenheit = group.Units.First(u => u.Abbreviation == "°F");

            var result = group.Convert(0m, celsius, fahrenheit);

            result.Should().Be(32m);
        }

        [Fact]
        public void Convert_FahrenheitToCelsius_CorrectMath()
        {
            var group = CreateTemperatureGroup();
            var celsius = group.Units.First(u => u.Abbreviation == "°C");
            var fahrenheit = group.Units.First(u => u.Abbreviation == "°F");

            var result = group.Convert(32m, fahrenheit, celsius);

            result.Should().Be(0m);
        }
    }

    public sealed class EventRaised
    {
        [Fact]
        public void AddUnit_RaisesUnitAddedEvent()
        {
            var group = GroupUnit.Create(Guid.NewGuid(), "Weight");

            group.AddUnit(Guid.NewGuid(), "Kilogram", "kg", isBaseUnit: true, exchangeRateToBaseUnit: 1m);

            group.DomainEvents.Should().Contain(e => e is GroupUnitUnitAddedDomainEvent);
            var evt = group.DomainEvents.OfType<GroupUnitUnitAddedDomainEvent>().Single();
            evt.GroupUnitId.Should().Be(group.Id);
            evt.UnitName.Should().Be("Kilogram");
        }

        [Fact]
        public void RemoveUnit_RaisesUnitRemovedEvent()
        {
            var group = GroupUnit.Create(Guid.NewGuid(), "Weight");
            var unit = group.AddUnit(Guid.NewGuid(), "Kilogram", "kg", isBaseUnit: true, exchangeRateToBaseUnit: 1m);
            group.ClearDomainEvents();

            group.RemoveUnit(unit);

            group.DomainEvents.Should().Contain(e => e is GroupUnitUnitRemovedDomainEvent);
            var evt = group.DomainEvents.OfType<GroupUnitUnitRemovedDomainEvent>().Single();
            evt.GroupUnitId.Should().Be(group.Id);
            evt.UnitId.Should().Be(unit.Id);
        }

        [Fact]
        public void Rename_RaisesGroupUnitRenamedDomainEvent()
        {
            var group = GroupUnit.Create(Guid.NewGuid(), "Weight");
            group.ClearDomainEvents();

            group.Rename("Mass");

            group.DomainEvents.Should().Contain(e => e is GroupUnitRenamedDomainEvent);
            var evt = group.DomainEvents.OfType<GroupUnitRenamedDomainEvent>().Single();
            evt.GroupUnitId.Should().Be(group.Id);
            evt.OldName.Should().Be("Weight");
            evt.NewName.Should().Be("Mass");
        }

        [Fact]
        public void Delete_RaisesGroupUnitDeletedDomainEvent()
        {
            var group = GroupUnit.Create(Guid.NewGuid(), "Weight");
            group.ClearDomainEvents();

            group.Delete();

            group.DomainEvents.Should().Contain(e => e is GroupUnitDeletedDomainEvent);
            var evt = group.DomainEvents.OfType<GroupUnitDeletedDomainEvent>().Single();
            evt.GroupUnitId.Should().Be(group.Id);
        }
    }
}
