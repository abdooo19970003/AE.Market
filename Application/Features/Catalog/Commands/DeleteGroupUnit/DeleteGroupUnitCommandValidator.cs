using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteGroupUnit;

public sealed class DeleteGroupUnitCommandValidator : AbstractValidator<DeleteGroupUnitCommand>
{
    public DeleteGroupUnitCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
