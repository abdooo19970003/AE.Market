using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.ReleaseVariantStock;

public sealed class ReleaseVariantStockCommandValidator : AbstractValidator<ReleaseVariantStockCommand>
{
    public ReleaseVariantStockCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
