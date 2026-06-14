using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteProductTaxCode;

internal sealed class DeleteProductTaxCodeCommandHandler(
    IRepository<ProductTaxCode> repo
) : IRequestHandler<DeleteProductTaxCodeCommand, Result>
{
    public async Task<Result> Handle(DeleteProductTaxCodeCommand request, CancellationToken cancellationToken)
    {
        var taxCode = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (taxCode is null)
            return Result.Fail(CatalogErrors.TaxCodeNotFound);

        taxCode.Delete();

        return Result.Success();
    }
}
