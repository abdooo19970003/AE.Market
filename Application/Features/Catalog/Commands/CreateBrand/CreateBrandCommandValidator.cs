using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Specifications;
using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.CreateBrand;

public sealed class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator(IReadRepository<Brand> repo)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(300)
            .MustAsync(async (slug, ct) => !await repo.AnyAsync(
                new BaseSpecification<Brand>(b => b.Slug == Domain.Aggregates.Catalog.ValueObjects.Slug.From(slug)), ct))
            .WithMessage("A brand with this slug already exists.");
        RuleFor(x => x.ShortDescription).MaximumLength(1000);
        RuleFor(x => x.LongDescription).MaximumLength(4000);
        RuleFor(x => x.LogoUrl).MaximumLength(1000);
    }
}
