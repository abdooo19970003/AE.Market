using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.Brands;

internal sealed class GetBrandByIdQueryHandler(
    IReadRepository<Brand> repo,
    IMapper mapper
) : IRequestHandler<GetBrandByIdQuery, Result<BrandDto>>
{
    public async Task<Result<BrandDto>> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brand = await repo.FirstOrDefaultAsync(new BrandByIdSpec(request.Id), cancellationToken);
        if (brand is null)
            return Result<BrandDto>.Fail(CatalogErrors.BrandNotFound);

        var dto = mapper.Map<BrandDto>(brand);
        return Result<BrandDto>.Success(dto);
    }
}
