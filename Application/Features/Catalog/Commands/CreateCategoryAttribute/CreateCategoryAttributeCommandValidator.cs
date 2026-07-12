using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Common.Specifications;
using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.CreateCategoryAttribute;

public sealed class CreateCategoryAttributeCommandValidator : AbstractValidator<CreateCategoryAttributeCommand>
{
    public CreateCategoryAttributeCommandValidator(IReadRepository<CategoryAttribute> repo)
    {
        RuleFor(x => x.AttributeName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.InputType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Slug).MaximumLength(300)
            .MustAsync(async (slug, ct) => slug is null || !await repo.AnyAsync(
                new BaseSpecification<CategoryAttribute>(a => a.Slug != null && a.Slug == Domain.Aggregates.Catalog.ValueObjects.Slug.From(slug)), ct))
            .WithMessage("A category attribute with this slug already exists.");
    }
}
