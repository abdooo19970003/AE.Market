using AE.Market.API.Authentication;
using AE.Market.API.Helpers;
using AE.Market.Application.Features.Catalog.Commands.ActivateProduct;
using AE.Market.Application.Features.Catalog.Commands.ActivateVariant;
using AE.Market.Application.Features.Catalog.Commands.AdjustVariantStock;
using AE.Market.Application.Features.Catalog.Commands.AddBundleItem;
using AE.Market.Application.Features.Catalog.Commands.AddProductImage;
using AE.Market.Application.Features.Catalog.Commands.AddProductRelation;
using AE.Market.Application.Features.Catalog.Commands.AddProductTag;
using AE.Market.Application.Features.Catalog.Commands.AddProductVariant;
using AE.Market.Application.Features.Catalog.Commands.CreateProduct;
using AE.Market.Application.Features.Catalog.Commands.DeleteProduct;
using AE.Market.Application.Features.Catalog.Commands.ReleaseVariantStock;
using AE.Market.Application.Features.Catalog.Commands.RemoveBundleItem;
using AE.Market.Application.Features.Catalog.Commands.RemoveProductAttribute;
using AE.Market.Application.Features.Catalog.Commands.RemoveProductImage;
using AE.Market.Application.Features.Catalog.Commands.RemoveProductRelation;
using AE.Market.Application.Features.Catalog.Commands.RemoveProductTag;
using AE.Market.Application.Features.Catalog.Commands.RemoveProductVariant;
using AE.Market.Application.Features.Catalog.Commands.ReserveVariantStock;
using AE.Market.Application.Features.Catalog.Commands.SetProductAttributeValue;
using AE.Market.Application.Features.Catalog.Commands.SetProductAttributeValues;
using AE.Market.Application.Features.Catalog.Commands.SetVariantAttributeValue;
using AE.Market.Application.Features.Catalog.Commands.SetVariantAttributeValues;
using AE.Market.Application.Features.Catalog.Commands.UpdateProduct;
using AE.Market.Application.Features.Catalog.Commands.UpdateVariantPricing;
using AE.Market.Application.Features.Catalog.Commands.UpdateVariantStock;
using AE.Market.Application.Features.Catalog.Queries.BundleItems;
using AE.Market.Application.Features.Catalog.Queries.ProductImages;
using AE.Market.Application.Features.Catalog.Queries.ProductRelations;
using AE.Market.Application.Features.Catalog.Queries.Products;
using AE.Market.Application.Features.Catalog.Queries.Tags;
using AE.Market.Domain.Aggregates.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts(CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductsListQuery(), ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id), ct);
        return result.ToNotFoundActionResult();
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetProductBySlug(string slug, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductBySlugQuery(slug), ct);
        return result.ToNotFoundActionResult();
    }

    [HttpGet("brand/{brandId:guid}")]
    public async Task<IActionResult> GetProductsByBrand(Guid brandId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductsByBrandQuery(brandId), ct);
        return result.ToActionResult();
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetProductsByCategory(Guid categoryId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductsByCategoryQuery(categoryId), ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand cmd, CancellationToken ct)
    {
        if (id != cmd.Id)
            return BadRequest("Id mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteProductCommand(id), ct);
        return result.ToDeletedActionResult();
    }

    [HttpPost("{productId:guid}/variants")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> AddVariant(Guid productId, [FromBody] AddProductVariantCommand cmd, CancellationToken ct)
    {
        if (productId != cmd.ProductId)
            return BadRequest("ProductId mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpPatch("{productId:guid}/variants/{variantId:guid}/pricing")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> UpdateVariantPricing(Guid productId, Guid variantId, [FromBody] UpdateVariantPricingCommand cmd, CancellationToken ct)
    {
        if (productId != cmd.ProductId || variantId != cmd.VariantId)
            return BadRequest("Id mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpPatch("{productId:guid}/variants/{variantId:guid}/stock")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> UpdateVariantStock(Guid productId, Guid variantId, [FromBody] UpdateVariantStockCommand cmd, CancellationToken ct)
    {
        if (productId != cmd.ProductId || variantId != cmd.VariantId)
            return BadRequest("Id mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{productId:guid}/variants/{variantId:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> RemoveVariant(Guid productId, Guid variantId, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveProductVariantCommand(productId, variantId), ct);
        return result.ToDeletedActionResult();
    }

    [HttpPost("{productId:guid}/variants/{variantId:guid}/reserve")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> ReserveVariantStock(Guid productId, Guid variantId, [FromBody] ReserveVariantStockCommand cmd, CancellationToken ct)
    {
        if (productId != cmd.ProductId || variantId != cmd.VariantId)
            return BadRequest("Id mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpPost("{productId:guid}/variants/{variantId:guid}/release")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> ReleaseVariantStock(Guid productId, Guid variantId, [FromBody] ReleaseVariantStockCommand cmd, CancellationToken ct)
    {
        if (productId != cmd.ProductId || variantId != cmd.VariantId)
            return BadRequest("Id mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpPost("{productId:guid}/variants/{variantId:guid}/adjust")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> AdjustVariantStock(Guid productId, Guid variantId, [FromBody] AdjustVariantStockCommand cmd, CancellationToken ct)
    {
        if (productId != cmd.ProductId || variantId != cmd.VariantId)
            return BadRequest("Id mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpPost("{productId:guid}/attributes")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> SetProductAttribute(Guid productId, [FromBody] SetProductAttributeValueCommand cmd, CancellationToken ct)
    {
        if (productId != cmd.ProductId)
            return BadRequest("ProductId mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{productId:guid}/attributes/{attributeValueId:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> RemoveProductAttribute(Guid productId, Guid attributeValueId, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveProductAttributeCommand(productId, attributeValueId), ct);
        return result.ToDeletedActionResult();
    }

    [HttpPut("{productId:guid}/attributes")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> SetProductAttributeValues(Guid productId, [FromBody] SetProductAttributeValuesCommand cmd, CancellationToken ct)
    {
        if (productId != cmd.ProductId)
            return BadRequest("ProductId mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpPost("{productId:guid}/variants/{variantId:guid}/attributes")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> SetVariantAttributeValue(Guid productId, Guid variantId, [FromBody] SetVariantAttributeValueCommand cmd, CancellationToken ct)
    {
        if (productId != cmd.ProductId || variantId != cmd.VariantId)
            return BadRequest("Id mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpPut("{productId:guid}/variants/{variantId:guid}/attributes")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> SetVariantAttributeValues(Guid productId, Guid variantId, [FromBody] SetVariantAttributeValuesCommand cmd, CancellationToken ct)
    {
        if (productId != cmd.ProductId || variantId != cmd.VariantId)
            return BadRequest("Id mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> ActivateProduct(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new ActivateProductCommand(id), ct);
        return result.ToActionResult();
    }

    [HttpPost("{productId:guid}/variants/{variantId:guid}/activate")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> ActivateVariant(Guid productId, Guid variantId, CancellationToken ct)
    {
        var result = await mediator.Send(new ActivateVariantCommand(productId, variantId), ct);
        return result.ToActionResult();
    }

    [HttpGet("tag/{tagSlug}")]
    public async Task<IActionResult> GetProductsByTag(string tagSlug, CancellationToken ct, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new GetProductsByTagQuery(tagSlug, page, pageSize), ct);
        return result.ToActionResult();
    }

    [HttpGet("{productId:guid}/bundle-items")]
    public async Task<IActionResult> GetBundleItems(Guid productId, CancellationToken ct, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new GetBundleItemsByBundleIdQuery(productId, page, pageSize), ct);
        return result.ToActionResult();
    }

    [HttpGet("{productId:guid}/bundle-items/{bundleItemId:guid}")]
    public async Task<IActionResult> GetBundleItemById(Guid productId, Guid bundleItemId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetBundleItemByIdQuery(bundleItemId), ct);
        return result.ToNotFoundActionResult();
    }

    [HttpPost("{productId:guid}/bundle-items")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> AddBundleItem(Guid productId, [FromBody] AddBundleItemCommand cmd, CancellationToken ct)
    {
        if (productId != cmd.ProductId)
            return BadRequest("ProductId mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpDelete("{productId:guid}/bundle-items/{bundleItemId:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> RemoveBundleItem(Guid productId, Guid bundleItemId, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveBundleItemCommand(productId, bundleItemId), ct);
        return result.ToDeletedActionResult();
    }

    [HttpGet("{productId:guid}/relations")]
    public async Task<IActionResult> GetProductRelations(Guid productId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductRelationsByProductQuery(productId), ct);
        return result.ToActionResult();
    }

    [HttpGet("{productId:guid}/relations/{relationId:guid}")]
    public async Task<IActionResult> GetProductRelationById(Guid productId, Guid relationId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductRelationByIdQuery(relationId), ct);
        return result.ToNotFoundActionResult();
    }

    [HttpPost("{productId:guid}/relations")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> AddProductRelation(Guid productId, [FromBody] AddProductRelationCommand cmd, CancellationToken ct)
    {
        if (productId != cmd.ProductId)
            return BadRequest("ProductId mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpDelete("{productId:guid}/relations/{relationId:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> RemoveProductRelation(Guid productId, Guid relationId, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveProductRelationCommand(productId, relationId), ct);
        return result.ToDeletedActionResult();
    }

    [HttpGet("{productId:guid}/related")]
    public async Task<IActionResult> GetRelatedProducts(Guid productId, [FromQuery] string type, CancellationToken ct, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var relationType = Enum.Parse<Domain.Aggregates.Catalog.Products.RelationType>(type);
        var result = await mediator.Send(new GetRelatedProductsByTypeQuery(productId, relationType, page, pageSize), ct);
        return result.ToActionResult();
    }

    [HttpGet("{productId:guid}/images")]
    public async Task<IActionResult> GetProductImages(Guid productId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductImagesByProductQuery(productId), ct);
        return result.ToActionResult();
    }

    [HttpGet("{productId:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> GetProductImageById(Guid productId, Guid imageId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductImageByIdQuery(imageId), ct);
        return result.ToNotFoundActionResult();
    }

    [HttpPost("{productId:guid}/images")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> AddProductImage(Guid productId, [FromBody] AddProductImageCommand cmd, CancellationToken ct)
    {
        if (productId != cmd.ProductId)
            return BadRequest("ProductId mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpDelete("{productId:guid}/images/{imageId:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> RemoveProductImage(Guid productId, Guid imageId, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveProductImageCommand(productId, imageId), ct);
        return result.ToDeletedActionResult();
    }

    [HttpGet("{productId:guid}/tags")]
    public async Task<IActionResult> GetProductTags(Guid productId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductTagsQuery(productId), ct);
        return result.ToActionResult();
    }

    [HttpPost("{productId:guid}/tags")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> AddProductTag(Guid productId, [FromBody] AddProductTagCommand cmd, CancellationToken ct)
    {
        if (productId != cmd.ProductId)
            return BadRequest("ProductId mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpDelete("{productId:guid}/tags/{slug}")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> RemoveProductTag(Guid productId, string slug, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveProductTagCommand(productId, slug), ct);
        return result.ToDeletedActionResult();
    }
}
