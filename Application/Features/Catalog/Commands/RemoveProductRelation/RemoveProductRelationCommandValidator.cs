using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveProductRelation;

public sealed class RemoveProductRelationCommandValidator : AbstractValidator<RemoveProductRelationCommand>
{
    public RemoveProductRelationCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.RelationId).NotEmpty();
    }
}
