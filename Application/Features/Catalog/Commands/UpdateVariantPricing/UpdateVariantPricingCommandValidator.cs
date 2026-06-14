using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateVariantPricing;

public sealed class UpdateVariantPricingCommandValidator : AbstractValidator<UpdateVariantPricingCommand>
{
    public UpdateVariantPricingCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0);
    }
}
