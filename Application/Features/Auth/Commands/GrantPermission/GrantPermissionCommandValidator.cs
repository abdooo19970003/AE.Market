using FluentValidation;

namespace AE.Market.Application.Features.Auth.Commands.GrantPermission
{
    internal sealed class GrantPermissionCommandValidator : AbstractValidator<GrantPermissionCommand>
    {
        public GrantPermissionCommandValidator()
        {
            RuleFor(c => c.UserId).NotEmpty();
            RuleFor(c => c.PermissionName).NotEmpty();
        }
    }
}
