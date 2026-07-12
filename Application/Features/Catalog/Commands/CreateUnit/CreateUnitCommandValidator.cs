using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.CreateUnit;

public sealed class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
{
    public CreateUnitCommandValidator()
    {
        RuleFor(x => x.UnitName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Abbreviation).NotEmpty().MaximumLength(20);
        RuleFor(x => x.GroupUnitId).NotEmpty();
        RuleFor(x => x.ExchangeRateToBaseUnit).GreaterThan(0);
        RuleFor(x => x.FormulaType).NotEmpty().MaximumLength(50);
    }
}
