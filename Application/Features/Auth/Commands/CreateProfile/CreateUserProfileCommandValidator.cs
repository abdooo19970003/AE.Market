using FluentValidation;

namespace AE.Market.Application.Features.Auth.Commands.CreateProfile;

internal sealed class CreateUserProfileCommandValidator : AbstractValidator<CreateUserProfileCommand>
{
    public CreateUserProfileCommandValidator()
    {
        RuleFor(c => c.FirstName)
            .NotEmpty()
            .MinimumLength(3);
    }
}
