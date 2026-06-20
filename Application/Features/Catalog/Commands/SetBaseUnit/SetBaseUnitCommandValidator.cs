using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.SetBaseUnit;

public sealed class SetBaseUnitCommandValidator : AbstractValidator<SetBaseUnitCommand>
{
    public SetBaseUnitCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
