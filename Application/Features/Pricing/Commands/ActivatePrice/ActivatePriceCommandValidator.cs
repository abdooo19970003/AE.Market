using FluentValidation;

namespace AE.Market.Application.Features.Pricing.Commands.ActivatePrice;

public sealed class ActivatePriceCommandValidator : AbstractValidator<ActivatePriceCommand>
{
    public ActivatePriceCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
    }
}
