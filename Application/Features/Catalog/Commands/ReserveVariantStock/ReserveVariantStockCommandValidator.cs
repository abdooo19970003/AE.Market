using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.ReserveVariantStock;

public sealed class ReserveVariantStockCommandValidator : AbstractValidator<ReserveVariantStockCommand>
{
    public ReserveVariantStockCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
