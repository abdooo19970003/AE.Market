using FluentValidation;

namespace AE.Market.Application.Features.Auth.Commands.Refresh
{
    internal sealed class RefreshCommandValidator : AbstractValidator<RefreshCommand>
    {
        public RefreshCommandValidator()
        {
            RuleFor(r => r.OldToken).NotEmpty().NotNull();
        }
    }
}
