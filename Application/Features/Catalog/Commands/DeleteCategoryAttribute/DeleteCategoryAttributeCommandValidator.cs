using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteCategoryAttribute;

public sealed class DeleteCategoryAttributeCommandValidator : AbstractValidator<DeleteCategoryAttributeCommand>
{
    public DeleteCategoryAttributeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
