using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.ProductImages;

internal sealed class GetProductImageByIdQueryHandler(
    IReadRepository<ProductImage> repo,
    IMapper mapper
) : IRequestHandler<GetProductImageByIdQuery, Result<ProductImageDto>>
{
    public async Task<Result<ProductImageDto>> Handle(GetProductImageByIdQuery request, CancellationToken cancellationToken)
    {
        var image = await repo.FirstOrDefaultAsync(new ProductImageByIdSpec(request.Id), cancellationToken);
        if (image is null)
            return Result<ProductImageDto>.Fail(CatalogErrors.ProductNotFound);

        var dto = mapper.Map<ProductImageDto>(image);
        return Result<ProductImageDto>.Success(dto);
    }
}
