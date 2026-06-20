namespace AE.Market.Application.Features.Catalog.DTOs;

public sealed record AttributeGroupDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<Guid> AttributeIds { get; set; } = [];
}
