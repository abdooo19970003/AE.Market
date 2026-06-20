namespace AE.Market.Application.Features.Catalog.DTOs;

public sealed record CategoryAttributeDto
{
    public Guid Id { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsFilterable { get; set; }
    public int SortOrder { get; set; }
    public Guid? DefaultUnitId { get; set; }
    public Guid? AllowedGroupUnitId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? AttributeGroupId { get; set; }
    public List<AttributeOptionDto> Options { get; set; } = [];
}
