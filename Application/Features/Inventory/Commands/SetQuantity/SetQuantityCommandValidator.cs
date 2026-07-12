using FluentValidation;

namespace AE.Market.Application.Features.Inventory.Commands.SetQuantity;

public sealed class SetQuantityCommandValidator : AbstractValidator<SetQuantityCommand>
{
    public SetQuantityCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
    }
}
