namespace AE.Market.Application.Features.Catalog.DTOs;

public sealed record GroupUnitDto
{
    public Guid Id { get; set; }
    public string GroupUnitName { get; set; } = string.Empty;
    public List<UnitDto> Units { get; set; } = [];
}

public sealed record UnitDto
{
    public Guid Id { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public bool IsBaseUnit { get; set; }
    public decimal ExchangeRateToBaseUnit { get; set; }
}
