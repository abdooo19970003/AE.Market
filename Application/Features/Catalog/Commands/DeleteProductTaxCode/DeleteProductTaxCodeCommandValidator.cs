using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteProductTaxCode;

public sealed class DeleteProductTaxCodeCommandValidator : AbstractValidator<DeleteProductTaxCodeCommand>
{
    public DeleteProductTaxCodeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
