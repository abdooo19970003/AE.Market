using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Common.Specifications;
using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.CreateAttributeGroup;

public sealed class CreateAttributeGroupCommandValidator : AbstractValidator<CreateAttributeGroupCommand>
{
    public CreateAttributeGroupCommandValidator(IReadRepository<AttributeGroup> repo)
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.GroupName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).MaximumLength(300)
            .MustAsync(async (slug, ct) => slug is null || !await repo.AnyAsync(
                new BaseSpecification<AttributeGroup>(g => g.Slug != null && g.Slug == Domain.Aggregates.Catalog.ValueObjects.Slug.From(slug)), ct))
            .WithMessage("An attribute group with this slug already exists.");
    }
}
