using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.ProductImages;

internal sealed class GetProductImagesByProductQueryHandler(
    IReadRepository<ProductImage> repo,
    IReadRepository<Product> productRepo,
    IMapper mapper
) : IRequestHandler<GetProductImagesByProductQuery, Result<List<ProductImageDto>>>
{
    public async Task<Result<List<ProductImageDto>>> Handle(GetProductImagesByProductQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<List<ProductImageDto>>.Fail(CatalogErrors.ProductNotFound);

        var spec = new ProductImagesByProductSpec(request.ProductId);
        var images = await repo.ListWithSpecAsync(spec, cancellationToken);
        var dtos = mapper.Map<List<ProductImageDto>>(images);

        return Result<List<ProductImageDto>>.Success(dtos);
    }
}
