namespace AE.Market.Application.Features.Cart.DTOs;

public sealed record CartDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid? SessionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<CartItemDto> Items { get; set; } = [];
    public int TotalItems => Items.Sum(i => i.Quantity);
    public DateTime CreatedAt { get; set; }
}
