using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.CreateBrand;

public sealed class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ShortDescription).MaximumLength(1000);
        RuleFor(x => x.LongDescription).MaximumLength(4000);
        RuleFor(x => x.LogoUrl).MaximumLength(1000);
    }
}
