using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.AssignAttributeToGroup;

public sealed class AssignAttributeToGroupCommandValidator : AbstractValidator<AssignAttributeToGroupCommand>
{
    public AssignAttributeToGroupCommandValidator()
    {
        RuleFor(x => x.AttributeId).NotEmpty();
    }
}
