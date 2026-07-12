using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.ActivateVariant;

public sealed class ActivateVariantCommandValidator : AbstractValidator<ActivateVariantCommand>
{
    public ActivateVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
    }
}
