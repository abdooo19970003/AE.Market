using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.SetVariantAttributeValue;

public sealed class SetVariantAttributeValueCommandValidator : AbstractValidator<SetVariantAttributeValueCommand>
{
    public SetVariantAttributeValueCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.AttributeId).NotEmpty();
        RuleFor(x => x.InputType).IsInEnum();
    }
}
