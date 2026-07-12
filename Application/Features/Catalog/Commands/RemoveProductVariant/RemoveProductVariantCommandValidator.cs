using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveProductVariant;

public sealed class RemoveProductVariantCommandValidator : AbstractValidator<RemoveProductVariantCommand>
{
    public RemoveProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
    }
}
