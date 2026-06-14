using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;

namespace AE.Market.Domain.Aggregates.Catalog.Units;

public sealed class Unit : BaseEntity
{
    public string UnitName { get; private set; }
    public string Abbreviation { get; private set; }
    public Guid GroupUnitId { get; private set; }
    public bool IsBaseUnit { get; private set; } = true;
    public decimal ExchangeRateToBaseUnit { get; private set; } = 1m;
    public FormulaType FormulaType { get; private set; } = FormulaType.None;
    public string? ConversionFormulaDescription { get; private set; }

    private Unit(Guid id, string unitName, string abbreviation, Guid groupUnitId,
        bool isBaseUnit, decimal exchangeRateToBaseUnit,
        FormulaType formulaType, string? conversionFormulaDescription)
        : base(id)
    {
        UnitName = unitName;
        Abbreviation = abbreviation;
        GroupUnitId = groupUnitId;
        IsBaseUnit = isBaseUnit;
        ExchangeRateToBaseUnit = exchangeRateToBaseUnit;
        FormulaType = formulaType;
        ConversionFormulaDescription = conversionFormulaDescription;
    }

    private Unit() { }

    internal static Unit Create(Guid id, string unitName, string abbreviation, Guid groupUnitId,
        bool isBaseUnit = false, decimal exchangeRateToBaseUnit = 1m,
        FormulaType formulaType = FormulaType.None,
        string? conversionFormulaDescription = null)
    {
        if (exchangeRateToBaseUnit <= 0m)
            throw new DomainException(CatalogErrors.UnitExchangeRateInvalid.Code, CatalogErrors.UnitExchangeRateInvalid.Message);

        return new Unit(id, unitName, abbreviation, groupUnitId, isBaseUnit, exchangeRateToBaseUnit,
            formulaType, conversionFormulaDescription);
    }

    internal void UpdateExchangeRate(decimal rate)
    {
        if (rate <= 0m)
            throw new DomainException(CatalogErrors.UnitExchangeRateInvalid.Code, CatalogErrors.UnitExchangeRateInvalid.Message);
        ExchangeRateToBaseUnit = rate;
        UpdateLastModified();
    }

    internal void SetAsBaseUnit()
    {
        IsBaseUnit = true;
        ExchangeRateToBaseUnit = 1m;
        FormulaType = FormulaType.None;
        ConversionFormulaDescription = null;
        UpdateLastModified();
    }
}
