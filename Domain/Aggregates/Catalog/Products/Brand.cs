using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Products;

public sealed class Brand : BaseEntity, IAggregateRoot, IMetaData
{
    public string Name { get; private set; }
    public Slug Slug { get; private set; }
    public string? ShortDescription { get; private set; }
    public string? LongDescription { get; private set; }
    public string? LogoUrl { get; private set; }
    public URL? WebsiteUrl { get; private set; }

    // SEO
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public string? MetaKeywords { get; private set; }

    // Control props
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private Brand(
        Guid id,
        string name,
        Slug slug,
        string? shortDescription,
        string? longDescription,
        string? logoUrl,
        URL? websiteUrl,
        int sortOrder
    )
        : base(id)
    {
        Name = name;
        Slug = slug;
        ShortDescription = shortDescription;
        LongDescription = longDescription;
        LogoUrl = logoUrl;
        WebsiteUrl = websiteUrl;
        SortOrder = sortOrder;
    }

    private Brand() { }

    public static Brand Create(
        Guid id,
        string name,
        string slug,
        string? shortDescription = null,
        string? longDescription = null,
        string? logoUrl = null,
        URL? websiteUrl = null,
        int sortOrder = 0
    )
    {
        var brand = new Brand(id, name, Slug.Create(slug), shortDescription, longDescription, logoUrl, websiteUrl, sortOrder);
        brand.AddDomainEvent(new BrandCreatedDomainEvent(brand.Id));
        return brand;
    }

    public void UpdateDetails(
        string name,
        string? shortDescription,
        string? longDescription,
        string? logoUrl,
        URL? websiteUrl,
        int sortOrder
    )
    {
        Name = name;
        ShortDescription = shortDescription;
        LongDescription = longDescription;
        LogoUrl = logoUrl;
        WebsiteUrl = websiteUrl;
        SortOrder = sortOrder;
        AddDomainEvent(new BrandDetailsUpdatedDomainEvent(Id, name, shortDescription, longDescription, logoUrl, websiteUrl?.Value, sortOrder));
        UpdateLastModified();
    }

    public void UpdateSlug(string slug)
    {
        var oldSlug = Slug;
        Slug = Slug.Create(slug);
        AddDomainEvent(new BrandSlugChangedDomainEvent(Id, oldSlug.Value, slug));
        UpdateLastModified();
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        AddDomainEvent(new BrandActivatedDomainEvent(Id));
        UpdateLastModified();
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        AddDomainEvent(new BrandDeactivatedDomainEvent(Id));
        UpdateLastModified();
    }

    public override void Delete()
    {
        AddDomainEvent(new BrandDeletedDomainEvent(Id));
        base.Delete();
    }

    public void SetOrUpdateMetaFields(string? title, string? description, string? keywords)
    {
        MetaTitle = title;
        MetaDescription = description;
        MetaKeywords = keywords;
        AddDomainEvent(new BrandMetaFieldsUpdatedDomainEvent(Id, title, description, keywords));
        UpdateLastModified();
    }
}
