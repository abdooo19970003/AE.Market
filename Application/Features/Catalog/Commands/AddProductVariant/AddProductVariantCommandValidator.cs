using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Specifications;
using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.AddProductVariant;

public sealed class AddProductVariantCommandValidator : AbstractValidator<AddProductVariantCommand>
{
    public AddProductVariantCommandValidator(
        IReadRepository<Product> repo,
        IReadRepository<ProductVariant> variantRepo)
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(500);
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
    }
}
