using FluentValidation;

namespace AE.Market.Application.Features.Pricing.Commands.DeactivatePrice;

public sealed class DeactivatePriceCommandValidator : AbstractValidator<DeactivatePriceCommand>
{
    public DeactivatePriceCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
    }
}
