using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveBundleItem;

public sealed class RemoveBundleItemCommandValidator : AbstractValidator<RemoveBundleItemCommand>
{
    public RemoveBundleItemCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.BundleItemId).NotEmpty();
    }
}
