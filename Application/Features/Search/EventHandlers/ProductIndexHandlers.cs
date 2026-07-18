using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Search.DTOs;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Aggregates.Catalog.Products;
using MediatR;

namespace AE.Market.Application.Features.Search.EventHandlers;

internal static class ProductDocumentMapper
{
    internal static async Task<ProductDocument> MapProductAsync(
        Product product,
        IReadRepository<Category> categoryRepo,
        IReadRepository<Brand> brandRepo,
        CancellationToken ct)
    {
        var category = await categoryRepo.GetByIdAsync(product.CategoryId, ct);
        var brand = product.BrandId != Guid.Empty
            ? await brandRepo.GetByIdAsync(product.BrandId, ct)
            : null;

        return new ProductDocument
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug.Value,
            Sku = product.Sku.Value,
            ShortDescription = product.ShortDescription,
            LongDescription = product.LongDescription,
            Details = product.Details,
            Status = product.Status.ToString(),
            ProductType = product.ProductType.ToString(),
            CategoryId = product.CategoryId,
            CategoryName = category?.CategoryName ?? string.Empty,
            BrandId = product.BrandId,
            BrandName = brand?.Name ?? string.Empty,
            Tags = [.. product.Tags.Select(t => t.Name)],
            ListPrice = product.ListPrice,
            StockQuantity = product.StockQuantity,
            AllowBackOrder = product.AllowBackOrder,
            MetaTitle = product.MetaTitle,
            MetaDescription = product.MetaDescription,
            MetaKeywords = product.MetaKeywords,
            Variants = [.. product.Variants.Select(v => new VariantDocument
            {
                Id = v.Id,
                Name = v.Name,
                Sku = v.Sku.Value,
                Status = v.Status.ToString(),
                ListPrice = v.ListPrice,
                StockQuantity = v.StockQuantity,
                AttributeValues = [.. v.AttributeValues.Select(av => new VariantAttributeValueDocument
                {
                    AttributeId = av.AttributeId,
                    AttributeName = string.Empty,
                    OptionValue = av.ValueText ?? string.Empty
                })]
            })],
            Images = [.. product.Images.Select(i => new ImageDocument
            {
                Url = i.Url,
                AltText = i.AltText,
                SortOrder = i.SortOrder
            })],
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.LastModified
        };
    }
}

internal sealed class ProductCreatedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.IndexProductAsync(doc, cancellationToken);
    }
}

internal sealed class ProductUpdatedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductUpdatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductDetailsUpdatedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductDetailsUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductDetailsUpdatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductActivatedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductActivatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductActivatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductDeactivatedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductDeactivatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductDeactivatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductDeletedIndexHandler(
    IElasticsearchService es)
    : INotificationHandler<DomainEventNotification<ProductDeletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductDeletedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        await es.DeleteProductAsync(notification.DomainEvent.ProductId, cancellationToken);
    }
}

internal sealed class ProductSlugChangedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductSlugChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductSlugChangedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductBrandChangedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductBrandChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductBrandChangedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductCategoryChangedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductCategoryChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductCategoryChangedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductTagAddedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductTagAddedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductTagAddedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductTagRemovedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductTagRemovedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductTagRemovedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductImageAddedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductImageAddedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductImageAddedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductImageRemovedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductImageRemovedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductImageRemovedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductVariantAddedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductVariantAddedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductVariantAddedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductVariantRemovedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductVariantRemovedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductVariantRemovedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class VariantActivatedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<VariantActivatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<VariantActivatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class VariantDeactivatedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<VariantDeactivatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<VariantDeactivatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class VariantPriceChangedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<VariantPriceChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<VariantPriceChangedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class VariantStockAdjustedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<VariantStockAdjustedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<VariantStockAdjustedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductShortDescriptionChangedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductShortDescriptionChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductShortDescriptionChangedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductLongDescriptionChangedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductLongDescriptionChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductLongDescriptionChangedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductMetaFieldsUpdatedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductMetaFieldsUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductMetaFieldsUpdatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductTypeChangedIndexHandler(
    IElasticsearchService es,
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo)
    : INotificationHandler<DomainEventNotification<ProductTypeChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductTypeChangedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null) return;

        var doc = await ProductDocumentMapper.MapProductAsync(product, categoryRepo, brandRepo, cancellationToken);
        await es.UpdateProductAsync(product.Id, doc, cancellationToken);
    }
}

internal sealed class ProductTaxCodeChangedIndexHandler
    : INotificationHandler<DomainEventNotification<ProductTaxCodeChangedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<ProductTaxCodeChangedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
