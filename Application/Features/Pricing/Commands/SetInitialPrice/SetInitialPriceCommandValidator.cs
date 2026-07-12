using FluentValidation;

namespace AE.Market.Application.Features.Pricing.Commands.SetInitialPrice;

public sealed class SetInitialPriceCommandValidator : AbstractValidator<SetInitialPriceCommand>
{
    public SetInitialPriceCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Z]{3}$");
        RuleFor(x => x.Type).IsInEnum();
    }
}
