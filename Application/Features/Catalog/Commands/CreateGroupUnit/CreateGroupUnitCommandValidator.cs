using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.CreateGroupUnit;

public sealed class CreateGroupUnitCommandValidator : AbstractValidator<CreateGroupUnitCommand>
{
    public CreateGroupUnitCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
