using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Specifications;
using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator(
        IReadRepository<Product> repo,
        IReadRepository<ProductVariant> variantRepo)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(500)
            .MustAsync(async (slug, ct) => !await repo.AnyAsync(
                new BaseSpecification<Product>(p => p.Slug == Domain.Aggregates.Catalog.ValueObjects.Slug.From(slug)), ct))
            .WithMessage("A product with this slug already exists.");
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50)
            .MustAsync(async (sku, ct) =>
            {
                var skuObj = Sku.Create(sku);
                var productExists = await repo.AnyAsync(
                    new BaseSpecification<Product>(p => p.Sku == skuObj), ct);
                if (productExists) return false;
                var variantExists = await variantRepo.AnyAsync(
                    new BaseSpecification<ProductVariant>(v => v.Sku == skuObj), ct);
                return !variantExists;
            })
            .WithMessage("A product or variant with this SKU already exists.");
        RuleFor(x => x.Details).MaximumLength(4000);
        RuleFor(x => x.ShortDescription).MaximumLength(1000);
        RuleFor(x => x.LongDescription).MaximumLength(4000);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.BrandId).NotEmpty();
        RuleFor(x => x.TaxCodeId).NotEmpty();
        RuleFor(x => x.ProductType).NotEmpty().IsEnumName(typeof(ProductType));
        RuleFor(x => x.BackOrderLimit).GreaterThan(0).When(x => x.AllowBackOrder);
    }
}
