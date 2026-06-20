using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Common.Specifications;
using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator(IReadRepository<Category> repo)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(300)
            .MustAsync(async (slug, ct) => !await repo.AnyAsync(
                new BaseSpecification<Category>(c => c.Slug == Domain.Aggregates.Catalog.ValueObjects.Slug.From(slug)), ct))
            .WithMessage("A category with this slug already exists.");
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
    }
}
