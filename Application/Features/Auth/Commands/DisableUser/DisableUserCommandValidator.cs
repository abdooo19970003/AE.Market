using FluentValidation;

namespace AE.Market.Application.Features.Auth.Commands.DisableUser;

internal sealed class DisableUserCommandValidator : AbstractValidator<DisableUserCommand>
{
    public DisableUserCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
    }
}
