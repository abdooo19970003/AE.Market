using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateBrand;

public sealed class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ShortDescription).MaximumLength(1000);
        RuleFor(x => x.LongDescription).MaximumLength(4000);
        RuleFor(x => x.LogoUrl).MaximumLength(1000);
    }
}
