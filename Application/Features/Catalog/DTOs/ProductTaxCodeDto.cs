namespace AE.Market.Application.Features.Catalog.DTOs;

public sealed record ProductTaxCodeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? PerformanceLocationRequirement { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
