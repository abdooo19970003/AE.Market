using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.Specifications;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IRepository<Product> repo,
    IReadRepository<ProductVariant> variantRepo,
    IReadRepository<Category> categoryRepo,
    IMapper mapper
) : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result<ProductDto>.Fail(CatalogErrors.CategoryNotFound);

        var sku = Sku.Create(request.Sku);
        var existingProduct = await repo.AnyAsync(
            new BaseSpecification<Product>(p => p.Sku == sku), cancellationToken);
        if (existingProduct)
            return Result<ProductDto>.Fail(CatalogErrors.ProductSkuAlreadyExists);

        var existingVariant = await variantRepo.AnyAsync(
            new BaseSpecification<ProductVariant>(v => v.Sku == sku), cancellationToken);
        if (existingVariant)
            return Result<ProductDto>.Fail(CatalogErrors.ProductSkuAlreadyExists);

        var productType = (ProductType)Enum.Parse(typeof(ProductType), request.ProductType);
        var product = Product.Create(
            Guid.NewGuid(),
            request.Name,
            request.Slug,
            request.Sku,
            request.CategoryId,
            productType,
            request.Details
        );

        product.UpdateBrand(request.BrandId);
        product.UpdateTaxCode(request.TaxCodeId);
        product.SetAllowBackOrder(request.AllowBackOrder, request.BackOrderLimit);
        product.SetShortDescription(request.ShortDescription);
        product.SetLongDescription(request.LongDescription);

        if (request.AttributeValues is not null)
        {
            foreach (var attr in request.AttributeValues)
            {
                product.SetAttributeValue(
                    Guid.NewGuid(),
                    attr.AttributeId,
                    attr.InputType,
                    attr.IsVariantDefiner,
                    attr.ValueText,
                    attr.ValueInteger,
                    attr.ValueDecimal,
                    attr.ValueBoolean,
                    attr.ValueDateTime,
                    attr.OptionId
                );
            }
        }

        await repo.AddAsync(product, cancellationToken);

        var dto = mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(dto);
    }
}
