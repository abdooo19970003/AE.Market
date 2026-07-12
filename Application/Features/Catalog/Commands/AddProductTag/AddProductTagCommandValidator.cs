using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Specifications;
using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.AddProductTag;

public sealed class AddProductTagCommandValidator : AbstractValidator<AddProductTagCommand>
{
    public AddProductTagCommandValidator(IReadRepository<Tag> repo)
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(300)
            .MustAsync(async (slug, ct) => !await repo.AnyAsync(
                new BaseSpecification<Tag>(t => t.Slug == Domain.Aggregates.Catalog.ValueObjects.Slug.From(slug)), ct))
            .WithMessage("A tag with this slug already exists.");
    }
}
