using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteUnit;

public sealed class DeleteUnitCommandValidator : AbstractValidator<DeleteUnitCommand>
{
    public DeleteUnitCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
