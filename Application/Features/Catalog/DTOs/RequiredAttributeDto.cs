namespace AE.Market.Application.Features.Catalog.DTOs;

public sealed record RequiredAttributeDto
{
    public Guid AttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsVariantDefiner { get; set; }
    public Guid? CurrentValueId { get; set; }
    public string? CurrentValue { get; set; }
}
