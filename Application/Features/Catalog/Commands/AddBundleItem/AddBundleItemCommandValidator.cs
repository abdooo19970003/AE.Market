using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.AddBundleItem;

public sealed class AddBundleItemCommandValidator : AbstractValidator<AddBundleItemCommand>
{
    public AddBundleItemCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
