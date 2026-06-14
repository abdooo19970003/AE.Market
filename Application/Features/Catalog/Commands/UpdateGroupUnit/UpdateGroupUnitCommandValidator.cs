using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateGroupUnit;

public sealed class UpdateGroupUnitCommandValidator : AbstractValidator<UpdateGroupUnitCommand>
{
    public UpdateGroupUnitCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
