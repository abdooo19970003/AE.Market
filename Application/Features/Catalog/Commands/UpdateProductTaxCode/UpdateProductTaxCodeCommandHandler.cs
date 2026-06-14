using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateProductTaxCode;

internal sealed class UpdateProductTaxCodeCommandHandler(
    IRepository<ProductTaxCode> repo,
    IMapper mapper
) : IRequestHandler<UpdateProductTaxCodeCommand, Result<ProductTaxCodeDto>>
{
    public async Task<Result<ProductTaxCodeDto>> Handle(UpdateProductTaxCodeCommand request, CancellationToken cancellationToken)
    {
        var taxCode = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (taxCode is null)
            return Result<ProductTaxCodeDto>.Fail(CatalogErrors.TaxCodeNotFound);

        taxCode.UpdateDetails(request.Code, request.Type, request.PerformanceLocationRequirement, request.Name, request.Description);

        var dto = mapper.Map<ProductTaxCodeDto>(taxCode);
        return Result<ProductTaxCodeDto>.Success(dto);
    }
}
