namespace AE.Market.Application.Features.Catalog.DTOs;

public sealed record ProductRelationDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid RelatedProductId { get; set; }
    public string? RelatedProductName { get; set; }
    public string Type { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
