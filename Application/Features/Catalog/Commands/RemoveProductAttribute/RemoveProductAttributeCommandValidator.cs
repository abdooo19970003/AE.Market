using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveProductAttribute;

public sealed class RemoveProductAttributeCommandValidator : AbstractValidator<RemoveProductAttributeCommand>
{
    public RemoveProductAttributeCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.AttributeValueId).NotEmpty();
    }
}
