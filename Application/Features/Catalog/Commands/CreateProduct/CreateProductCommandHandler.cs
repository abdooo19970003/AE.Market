using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
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
    IMapper mapper
) : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
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

        await repo.AddAsync(product, cancellationToken);

        var dto = mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(dto);
    }
}
