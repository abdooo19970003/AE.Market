using FluentValidation;

namespace AE.Market.Application.Features.Pricing.Commands.DeletePrice;

public sealed class DeletePriceCommandValidator : AbstractValidator<DeletePriceCommand>
{
    public DeletePriceCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
    }
}
