using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteBrand;

internal sealed class DeleteBrandCommandHandler(
    IRepository<Brand> repo
) : IRequestHandler<DeleteBrandCommand, Result>
{
    public async Task<Result> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (brand is null)
            return Result.Fail(CatalogErrors.BrandNotFound);

        brand.Delete();

        return Result.Success();
    }
}
