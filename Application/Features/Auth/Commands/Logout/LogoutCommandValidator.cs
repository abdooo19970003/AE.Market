using FluentValidation;

namespace AE.Market.Application.Features.Auth.Commands.Logout
{
    public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
        {
        }
    }
}
