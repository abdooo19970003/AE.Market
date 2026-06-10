using FluentValidation;

namespace AE.Market.Application.Features.Auth.Commands.UpdateProfile;

internal sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(c => c.FirstName)
            .MinimumLength(3)
            .When(c => !string.IsNullOrEmpty(c.FirstName));
    }
}
