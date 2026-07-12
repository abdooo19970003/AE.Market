using FluentValidation;

namespace AE.Market.Application.Features.Inventory.Commands.UpdateInventoryItem;

public sealed class UpdateInventoryItemCommandValidator : AbstractValidator<UpdateInventoryItemCommand>
{
    public UpdateInventoryItemCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.LowStockThreshold)
            .GreaterThanOrEqualTo(0).When(x => x.LowStockThreshold.HasValue);
        RuleFor(x => x.BackorderLimit)
            .GreaterThan(0).When(x => x.BackorderLimit.HasValue);
        RuleFor(x => x.ShippingDimensions!.WeightInGrams)
            .GreaterThanOrEqualTo(0).When(x => x.ShippingDimensions is not null);
        RuleFor(x => x.ShippingDimensions!.LongInCentimeter)
            .GreaterThanOrEqualTo(0).When(x => x.ShippingDimensions is not null);
        RuleFor(x => x.ShippingDimensions!.HeightInCentimeter)
            .GreaterThanOrEqualTo(0).When(x => x.ShippingDimensions is not null);
        RuleFor(x => x.ShippingDimensions!.WidthInCentimeter)
            .GreaterThanOrEqualTo(0).When(x => x.ShippingDimensions is not null);
    }
}
