using FluentValidation;

namespace AE.Market.Application.Features.Inventory.Commands.DeleteInventoryItem;

public sealed class DeleteInventoryItemCommandValidator : AbstractValidator<DeleteInventoryItemCommand>
{
    public DeleteInventoryItemCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
    }
}
