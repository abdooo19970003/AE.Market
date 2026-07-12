using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteCategory;

internal sealed class DeleteCategoryCommandHandler(
    IRepository<Category> repo
) : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.Fail(CatalogErrors.CategoryNotFound);

        category.Delete();

        return Result.Success();
    }
}
