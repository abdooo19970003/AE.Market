using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Products;

public sealed class Tag : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public Slug Slug { get; private set; } = null!;

    private Tag(Guid id, string name, Slug slug)
        : base(id)
    {
        Name = name;
        Slug = slug;
    }

    private Tag() { }

    internal static Tag Create(Guid id, string name, string slug)
    {
        return new Tag(id, name, Slug.Create(slug));
    }
}
