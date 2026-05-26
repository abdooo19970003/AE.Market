using FluentValidation;

namespace AE.Market.Application.Features.Auth.Commands.Login
{
    internal class LoginCommandValidator :AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(c => c.Email).EmailAddress().NotNull();
            RuleFor(c => c.Password).NotNull();
        }
    }
}
