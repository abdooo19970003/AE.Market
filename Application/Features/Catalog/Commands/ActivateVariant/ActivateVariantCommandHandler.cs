using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.ActivateVariant;

internal sealed class ActivateVariantCommandHandler(
    IRepository<Product> repo,
    IReadRepository<Category> categoryRepo
) : IRequestHandler<ActivateVariantCommand, Result>
{
    public async Task<Result> Handle(ActivateVariantCommand request, CancellationToken cancellationToken)
    {
        var spec = new ProductByIdSpec(request.ProductId, includeChildren: true);
        var product = await repo.GetBySpecWithTrackingAsync(spec, cancellationToken);
        if (product is null)
            return Result.Fail(CatalogErrors.ProductNotFound);

        var categorySpec = new CategoryByIdSpec(product.CategoryId, includeChildren: true);
        var category = await categoryRepo.FirstOrDefaultAsync(categorySpec, cancellationToken);
        if (category is null)
            return Result.Fail(CatalogErrors.CategoryNotFound);

        var requiredAttributeIds = category.Attributes
            .Where(a => !a.IsDeleted && a.IsRequired)
            .Select(a => a.Id)
            .ToList();

        product.ActivateVariant(
            request.VariantId,
            requiredAttributeIds.Count > 0 ? requiredAttributeIds.AsReadOnly() : null
        );

        return Result.Success();
    }
}
