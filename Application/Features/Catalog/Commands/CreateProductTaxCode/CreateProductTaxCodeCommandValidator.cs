using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.CreateProductTaxCode;

public sealed class CreateProductTaxCodeCommandValidator : AbstractValidator<CreateProductTaxCodeCommand>
{
    public CreateProductTaxCodeCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.PerformanceLocationRequirement).MaximumLength(500);
    }
}
