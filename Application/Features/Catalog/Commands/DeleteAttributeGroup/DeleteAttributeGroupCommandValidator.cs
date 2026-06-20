using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteAttributeGroup;

public sealed class DeleteAttributeGroupCommandValidator : AbstractValidator<DeleteAttributeGroupCommand>
{
    public DeleteAttributeGroupCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
