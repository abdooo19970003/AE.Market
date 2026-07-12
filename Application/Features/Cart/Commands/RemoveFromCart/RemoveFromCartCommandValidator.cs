using FluentValidation;

namespace AE.Market.Application.Features.Cart.Commands.RemoveFromCart;

public sealed class RemoveFromCartCommandValidator : AbstractValidator<RemoveFromCartCommand>
{
    public RemoveFromCartCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
    }
}
