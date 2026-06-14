using FluentValidation;

namespace AE.Market.Application.Features.Auth.Commands.RevokePermission;

internal sealed class RevokePermissionCommandValidator : AbstractValidator<RevokePermissionCommand>
{
    public RevokePermissionCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.PermissionName).NotEmpty();
    }
}
