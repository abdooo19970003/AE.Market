using FluentValidation;

namespace AE.Market.Application.Features.Auth.Commands.EnableUser;

internal sealed class EnableUserCommandValidator : AbstractValidator<EnableUserCommand>
{
    public EnableUserCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
    }
}
