using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.ActivateProduct;

public sealed class ActivateProductCommandValidator : AbstractValidator<ActivateProductCommand>
{
    public ActivateProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
