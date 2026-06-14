using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.AddProductVariant;

public sealed class AddProductVariantCommandValidator : AbstractValidator<AddProductVariantCommand>
{
    public AddProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
    }
}
