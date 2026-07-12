namespace AE.Market.Application.Features.Catalog.DTOs;

public sealed record AttributeOptionDto
{
    public Guid Id { get; set; }
    public Guid AttributeId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
