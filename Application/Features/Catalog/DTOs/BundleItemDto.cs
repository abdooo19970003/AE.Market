namespace AE.Market.Application.Features.Catalog.DTOs;

public sealed record BundleItemDto
{
    public Guid Id { get; set; }
    public Guid BundleId { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public int Quantity { get; set; }
}
