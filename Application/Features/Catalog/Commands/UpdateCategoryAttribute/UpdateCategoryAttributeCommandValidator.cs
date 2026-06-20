using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateCategoryAttribute;

public sealed class UpdateCategoryAttributeCommandValidator : AbstractValidator<UpdateCategoryAttributeCommand>
{
    public UpdateCategoryAttributeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AttributeName).NotEmpty().MaximumLength(200);
    }
}
