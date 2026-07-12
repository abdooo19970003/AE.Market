using FluentValidation;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteBrand;

public sealed class DeleteBrandCommandValidator : AbstractValidator<DeleteBrandCommand>
{
    public DeleteBrandCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
