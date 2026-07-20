using FluentValidation;

namespace AE.Market.Application.Features.Analytics.Commands.RecordSearchQuery;

public sealed class RecordSearchQueryCommandValidator : AbstractValidator<RecordSearchQueryCommand>
{
    public RecordSearchQueryCommandValidator()
    {
        RuleFor(x => x.SearchText)
            .NotEmpty()
            .MaximumLength(500);
        RuleFor(x => x.ResultCount)
            .GreaterThanOrEqualTo(0);
        RuleFor(x => x.LatencyMs)
            .GreaterThanOrEqualTo(0);
    }
}
