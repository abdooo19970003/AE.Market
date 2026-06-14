using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.ProductTaxCodes;

internal sealed class GetProductTaxCodesListQueryHandler(
    IReadRepository<ProductTaxCode> repo,
    IMapper mapper
) : IRequestHandler<GetProductTaxCodesListQuery, Result<List<ProductTaxCodeDto>>>
{
    public async Task<Result<List<ProductTaxCodeDto>>> Handle(GetProductTaxCodesListQuery request, CancellationToken cancellationToken)
    {
        var taxCodes = await repo.ListAsync(cancellationToken);
        var dtos = mapper.Map<List<ProductTaxCodeDto>>(taxCodes);
        return Result<List<ProductTaxCodeDto>>.Success(dtos);
    }
}
