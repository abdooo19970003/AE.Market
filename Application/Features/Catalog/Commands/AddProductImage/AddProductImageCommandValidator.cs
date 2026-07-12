using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.AddProductImage;

public sealed class AddProductImageCommandValidator : AbstractValidator<AddProductImageCommand>
{
    public AddProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Url).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.AltText).MaximumLength(500);
    }
}
