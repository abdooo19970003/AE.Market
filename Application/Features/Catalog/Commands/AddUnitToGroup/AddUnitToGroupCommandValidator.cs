using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.AddUnitToGroup;

public sealed class AddUnitToGroupCommandValidator : AbstractValidator<AddUnitToGroupCommand>
{
    public AddUnitToGroupCommandValidator()
    {
        RuleFor(x => x.GroupUnitId).NotEmpty();
        RuleFor(x => x.UnitName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Abbreviation).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ExchangeRateToBaseUnit).GreaterThan(0);
    }
}
