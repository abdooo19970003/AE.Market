using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.Brands;

internal sealed class GetBrandsListQueryHandler(
    IReadRepository<Brand> repo,
    IMapper mapper
) : IRequestHandler<GetBrandsListQuery, Result<List<BrandDto>>>
{
    public async Task<Result<List<BrandDto>>> Handle(GetBrandsListQuery request, CancellationToken cancellationToken)
    {
        var brands = await repo.ListAsync(cancellationToken);
        var dtos = mapper.Map<List<BrandDto>>(brands);
        return Result<List<BrandDto>>.Success(dtos);
    }
}
