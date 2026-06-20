using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.SetProductAttributeValue;

public sealed class SetProductAttributeValueCommandValidator : AbstractValidator<SetProductAttributeValueCommand>
{
    public SetProductAttributeValueCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.AttributeId).NotEmpty();
        RuleFor(x => x.InputType).IsInEnum();
    }
}
