using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;

namespace AE.Market.Domain.Aggregates.Catalog.Units;

public sealed class GroupUnit : BaseEntity, IAggregateRoot
{
    public string GroupUnitName { get; private set; }

    private readonly List<Unit> _units = [];
    public IReadOnlyCollection<Unit> Units => _units.AsReadOnly();
    public string BaseUnitAbbreviation => _units.FirstOrDefault(u => u.IsBaseUnit)?.Abbreviation ?? string.Empty;

    private GroupUnit(Guid id, string groupUnitName)
        : base(id)
    {
        GroupUnitName = groupUnitName;
    }

    private GroupUnit() { }

    public static GroupUnit Create(Guid id, string groupUnitName)
    {
        var groupUnit = new GroupUnit(id, groupUnitName);
        groupUnit.AddDomainEvent(new GroupUnitCreatedDomainEvent(groupUnit.Id));
        return groupUnit;
    }

    internal Unit AddUnit(Guid unitId, string unitName, string abbreviation,
        bool isBaseUnit = false, decimal exchangeRateToBaseUnit = 1m,
        FormulaType formulaType = FormulaType.None,
        string? conversionFormulaDescription = null)
    {
        if (isBaseUnit && _units.Any(u => u.IsBaseUnit))
            throw new DomainException(CatalogErrors.GroupUnitAlreadyHasBaseUnit.Code, CatalogErrors.GroupUnitAlreadyHasBaseUnit.Message);

        var unit = Unit.Create(unitId, unitName, abbreviation, Id, isBaseUnit, exchangeRateToBaseUnit,
            formulaType, conversionFormulaDescription);
        _units.Add(unit);
        AddDomainEvent(new GroupUnitUnitAddedDomainEvent(Id, unitId, unitName, abbreviation, isBaseUnit));
        UpdateLastModified();
        return unit;
    }

    internal void RemoveUnit(Unit unit)
    {
        unit.Delete();
        _units.Remove(unit);
        AddDomainEvent(new GroupUnitUnitRemovedDomainEvent(Id, unit.Id));
        UpdateLastModified();
    }

    public void Rename(string name)
    {
        var oldName = GroupUnitName;
        GroupUnitName = name;
        AddDomainEvent(new GroupUnitRenamedDomainEvent(Id, oldName, name));
        UpdateLastModified();
    }

    public decimal Convert(decimal value, Unit from, Unit to)
    {
        if (from.GroupUnitId != Id || to.GroupUnitId != Id)
            throw new DomainException("Catalog.Unit.DifferentGroup", "Cannot convert between different unit groups.");

        if (from.Id == to.Id)
            return value;

        decimal baseValue = ToBase(value, from);
        return FromBase(baseValue, to);
    }

    private static decimal ToBase(decimal value, Unit unit)
    {
        return unit.FormulaType switch
        {
            FormulaType.CelsiusToFahrenheit => value * 9m / 5m + 32m,
            FormulaType.FahrenheitToCelsius => (value - 32m) * 5m / 9m,
            _ => value * unit.ExchangeRateToBaseUnit
        };
    }

    private static decimal FromBase(decimal value, Unit unit)
    {
        return unit.FormulaType switch
        {
            FormulaType.CelsiusToFahrenheit => (value - 32m) * 5m / 9m,
            FormulaType.FahrenheitToCelsius => value * 9m / 5m + 32m,
            _ => value / unit.ExchangeRateToBaseUnit
        };
    }

    public override void Delete()
    {
        foreach (var unit in _units)
            unit.Delete();
        AddDomainEvent(new GroupUnitDeletedDomainEvent(Id));
        base.Delete();
    }
}
