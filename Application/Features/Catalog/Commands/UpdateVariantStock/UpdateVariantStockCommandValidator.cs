using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateVariantStock;

public sealed class UpdateVariantStockCommandValidator : AbstractValidator<UpdateVariantStockCommand>
{
    public UpdateVariantStockCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}
