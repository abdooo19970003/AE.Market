using FluentValidation;

namespace AE.Market.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(c => c.Email)
                .EmailAddress()
                .NotEmpty();
            RuleFor(c => c.Password)
                .NotEmpty()
                .MinimumLength(4)
                //.Matches("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{8,}$")
                ;
        }
    }
}
