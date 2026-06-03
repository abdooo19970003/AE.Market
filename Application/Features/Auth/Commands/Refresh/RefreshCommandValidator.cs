using FluentValidation;

namespace AE.Market.Application.Features.Auth.Commands.Refresh
{
    internal class RefreshCommandValidator : AbstractValidator<RefreshCommand>
    {
        public RefreshCommandValidator()
        {
            RuleFor(r => r.OldToken).NotEmpty().NotNull();
        }
    }
}
