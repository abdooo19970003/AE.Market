using FluentValidation;

namespace AE.Market.Application.Features.Analytics.Commands.RecordProductView;

public sealed class RecordProductViewCommandValidator : AbstractValidator<RecordProductViewCommand>
{
    public RecordProductViewCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
