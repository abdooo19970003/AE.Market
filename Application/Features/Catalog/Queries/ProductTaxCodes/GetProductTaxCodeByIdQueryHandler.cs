using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.ProductTaxCodes;

internal sealed class GetProductTaxCodeByIdQueryHandler(
    IReadRepository<ProductTaxCode> repo,
    IMapper mapper
) : IRequestHandler<GetProductTaxCodeByIdQuery, Result<ProductTaxCodeDto>>
{
    public async Task<Result<ProductTaxCodeDto>> Handle(GetProductTaxCodeByIdQuery request, CancellationToken cancellationToken)
    {
        var taxCode = await repo.FirstOrDefaultAsync(new ProductTaxCodeByIdSpec(request.Id), cancellationToken);
        if (taxCode is null)
            return Result<ProductTaxCodeDto>.Fail(CatalogErrors.TaxCodeNotFound);

        var dto = mapper.Map<ProductTaxCodeDto>(taxCode);
        return Result<ProductTaxCodeDto>.Success(dto);
    }
}
