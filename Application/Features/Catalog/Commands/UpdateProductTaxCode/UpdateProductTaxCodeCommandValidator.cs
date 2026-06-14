using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateProductTaxCode;

public sealed class UpdateProductTaxCodeCommandValidator : AbstractValidator<UpdateProductTaxCodeCommand>
{
    public UpdateProductTaxCodeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.PerformanceLocationRequirement).MaximumLength(500);
    }
}
