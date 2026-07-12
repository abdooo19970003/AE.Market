using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Specifications;
using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator(IReadRepository<Product> repo)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(500)
            .MustAsync(async (cmd, slug, ct) => !await repo.AnyAsync(
                new BaseSpecification<Product>(p => p.Slug == Domain.Aggregates.Catalog.ValueObjects.Slug.From(slug) && p.Id != cmd.Id), ct))
            .WithMessage("A product with this slug already exists.");
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
