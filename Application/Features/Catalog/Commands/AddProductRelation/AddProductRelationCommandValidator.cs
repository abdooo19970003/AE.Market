using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.AddProductRelation;

public sealed class AddProductRelationCommandValidator : AbstractValidator<AddProductRelationCommand>
{
    public AddProductRelationCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.RelatedProductId).NotEmpty();
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
    }
}
