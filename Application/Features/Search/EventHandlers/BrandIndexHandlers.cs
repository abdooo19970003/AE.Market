using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Application.Features.Search.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Aggregates.Catalog.Products;
using MediatR;

namespace AE.Market.Application.Features.Search.EventHandlers;

internal static class BrandDocumentMapper
{
    internal static async Task<BrandDocument> MapBrandAsync(
        Brand brand,
        IReadRepository<Product> productRepo,
        CancellationToken ct)
    {
        var productCount = await productRepo.CountAsync(
            new ProductsByBrandSpec(brand.Id), ct);

        return new BrandDocument
        {
            Id = brand.Id,
            Name = brand.Name,
            Slug = brand.Slug.Value,
            ShortDescription = brand.ShortDescription,
            LongDescription = brand.LongDescription,
            LogoUrl = brand.LogoUrl,
            IsActive = brand.IsActive,
            ProductCount = productCount
        };
    }
}

internal sealed class BrandCreatedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Brand> brandRepo,
    IReadRepository<Product> productRepo)
    : INotificationHandler<DomainEventNotification<BrandCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<BrandCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var brand = await brandRepo.GetByIdAsync(notification.DomainEvent.BrandId, cancellationToken);
        if (brand is null) return;

        var doc = await BrandDocumentMapper.MapBrandAsync(brand, productRepo, cancellationToken);
        await es.IndexBrandAsync(doc, cancellationToken);
    }
}

internal sealed class BrandDetailsUpdatedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Brand> brandRepo,
    IReadRepository<Product> productRepo)
    : INotificationHandler<DomainEventNotification<BrandDetailsUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<BrandDetailsUpdatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var brand = await brandRepo.GetByIdAsync(notification.DomainEvent.BrandId, cancellationToken);
        if (brand is null) return;

        var doc = await BrandDocumentMapper.MapBrandAsync(brand, productRepo, cancellationToken);
        await es.UpdateBrandAsync(brand.Id, doc, cancellationToken);
    }
}

internal sealed class BrandSlugChangedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Brand> brandRepo,
    IReadRepository<Product> productRepo)
    : INotificationHandler<DomainEventNotification<BrandSlugChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<BrandSlugChangedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var brand = await brandRepo.GetByIdAsync(notification.DomainEvent.BrandId, cancellationToken);
        if (brand is null) return;

        var doc = await BrandDocumentMapper.MapBrandAsync(brand, productRepo, cancellationToken);
        await es.UpdateBrandAsync(brand.Id, doc, cancellationToken);
    }
}

internal sealed class BrandActivatedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Brand> brandRepo,
    IReadRepository<Product> productRepo)
    : INotificationHandler<DomainEventNotification<BrandActivatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<BrandActivatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var brand = await brandRepo.GetByIdAsync(notification.DomainEvent.BrandId, cancellationToken);
        if (brand is null) return;

        var doc = await BrandDocumentMapper.MapBrandAsync(brand, productRepo, cancellationToken);
        await es.UpdateBrandAsync(brand.Id, doc, cancellationToken);
    }
}

internal sealed class BrandDeactivatedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Brand> brandRepo,
    IReadRepository<Product> productRepo)
    : INotificationHandler<DomainEventNotification<BrandDeactivatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<BrandDeactivatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var brand = await brandRepo.GetByIdAsync(notification.DomainEvent.BrandId, cancellationToken);
        if (brand is null) return;

        var doc = await BrandDocumentMapper.MapBrandAsync(brand, productRepo, cancellationToken);
        await es.UpdateBrandAsync(brand.Id, doc, cancellationToken);
    }
}

internal sealed class BrandMetaFieldsUpdatedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Brand> brandRepo,
    IReadRepository<Product> productRepo)
    : INotificationHandler<DomainEventNotification<BrandMetaFieldsUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<BrandMetaFieldsUpdatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var brand = await brandRepo.GetByIdAsync(notification.DomainEvent.BrandId, cancellationToken);
        if (brand is null) return;

        var doc = await BrandDocumentMapper.MapBrandAsync(brand, productRepo, cancellationToken);
        await es.UpdateBrandAsync(brand.Id, doc, cancellationToken);
    }
}

internal sealed class BrandDeletedIndexHandler(
    IElasticsearchService es)
    : INotificationHandler<DomainEventNotification<BrandDeletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<BrandDeletedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        await es.DeleteBrandAsync(notification.DomainEvent.BrandId, cancellationToken);
    }
}
