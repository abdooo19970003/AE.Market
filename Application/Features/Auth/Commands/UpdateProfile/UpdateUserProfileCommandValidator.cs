using FluentValidation;

namespace AE.Market.Application.Features.Auth.Commands.UpdateProfile;

internal sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(c => c.FirstName)
            .MinimumLength(3)
            .When(c => !string.IsNullOrEmpty(c.FirstName));

        RuleFor(c => c.Addresses)
            .Null()
            .When(c => c.Addresses is null);

        RuleForEach(c => c.Addresses).ChildRules(addr =>
        {
            addr.RuleFor(a => a.Country).NotEmpty();
            addr.RuleFor(a => a.City).NotEmpty();
        });
    }
}
