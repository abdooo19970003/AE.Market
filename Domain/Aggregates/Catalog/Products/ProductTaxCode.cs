using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Products;

public sealed class ProductTaxCode : BaseEntity, IAggregateRoot
{
    private ProductTaxCode() { }

    private ProductTaxCode(
        Guid id,
        string code,
        string type,
        string? performanceLocationRequirement,
        string name,
        string description
    )
        : base(id)
    {
        Code = code;
        Type = type;
        PerformanceLocationRequirement = performanceLocationRequirement;
        Name = name;
        Description = description;
    }

    public static ProductTaxCode Create(
        Guid id,
        string code,
        string type,
        string? performanceLocationRequirement,
        string name,
        string description
    )
    {
        var taxCode = new ProductTaxCode(id, code, type, performanceLocationRequirement, name, description);
        taxCode.AddDomainEvent(new ProductTaxCodeCreatedDomainEvent(taxCode.Id));
        return taxCode;
    }

    public string Code { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string? PerformanceLocationRequirement { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public override void Delete()
    {
        AddDomainEvent(new ProductTaxCodeDeletedDomainEvent(Id));
        base.Delete();
    }

    public override void Restore()
    {
        base.Restore();
    }

    public void UpdateDetails(
        string code,
        string type,
        string? performanceLocationRequirement,
        string name,
        string description
    )
    {
        Code = code;
        Type = type;
        PerformanceLocationRequirement = performanceLocationRequirement;
        Name = name;
        Description = description;
        AddDomainEvent(new ProductTaxCodeUpdatedDomainEvent(Id));
        UpdateLastModified();
    }
}
