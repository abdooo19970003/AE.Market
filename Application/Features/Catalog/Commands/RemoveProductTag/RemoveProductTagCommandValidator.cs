using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveProductTag;

public sealed class RemoveProductTagCommandValidator : AbstractValidator<RemoveProductTagCommand>
{
    public RemoveProductTagCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty();
    }
}
