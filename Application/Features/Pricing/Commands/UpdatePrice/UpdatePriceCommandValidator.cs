using AE.Market.Domain.Aggregates.Prices;
using FluentValidation;

namespace AE.Market.Application.Features.Pricing.Commands.UpdatePrice;

public sealed class UpdatePriceCommandValidator : AbstractValidator<UpdatePriceCommand>
{
    public UpdatePriceCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Z]{3}$");
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Reason).IsInEnum();
    }
}
