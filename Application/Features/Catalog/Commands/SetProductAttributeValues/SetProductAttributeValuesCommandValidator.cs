using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.SetProductAttributeValues;

public sealed class SetProductAttributeValuesCommandValidator : AbstractValidator<SetProductAttributeValuesCommand>
{
    public SetProductAttributeValuesCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.AttributeValues).NotNull();
    }
}
