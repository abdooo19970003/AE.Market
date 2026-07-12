using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.SetVariantAttributeValues;

public sealed class SetVariantAttributeValuesCommandValidator : AbstractValidator<SetVariantAttributeValuesCommand>
{
    public SetVariantAttributeValuesCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.AttributeValues).NotNull();
    }
}
