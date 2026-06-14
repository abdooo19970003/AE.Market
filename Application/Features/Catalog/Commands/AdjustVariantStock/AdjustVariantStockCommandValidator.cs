using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.AdjustVariantStock;

public sealed class AdjustVariantStockCommandValidator : AbstractValidator<AdjustVariantStockCommand>
{
    public AdjustVariantStockCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
    }
}
