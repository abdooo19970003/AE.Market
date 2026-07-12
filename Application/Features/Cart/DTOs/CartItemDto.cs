namespace AE.Market.Application.Features.Cart.DTOs;

public sealed record CartItemDto
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; }
}
