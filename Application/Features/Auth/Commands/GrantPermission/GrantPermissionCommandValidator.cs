using FluentValidation;

namespace AE.Market.Application.Features.Auth.Commands.GrantPermission
{
    internal class GrantPermissionCommandValidator : AbstractValidator<GrantPermissionCommand>
    {
        public GrantPermissionCommandValidator()
        {
            RuleFor(c => c.UserId).NotEmpty();
            RuleFor(c => c.Permission).NotEmpty();
        }
    }
}
