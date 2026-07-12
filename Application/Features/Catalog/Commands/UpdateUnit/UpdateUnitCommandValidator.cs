using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateUnit;

public sealed class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
{
    public UpdateUnitCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UnitName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Abbreviation).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ExchangeRateToBaseUnit).GreaterThan(0);
    }
}
