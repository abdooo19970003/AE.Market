using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.Products;

internal sealed class GetProductByIdQueryHandler(
    IReadRepository<Product> repo,
    IReadRepository<Brand> brandRepo,
    IReadRepository<Category> categoryRepo,
    IMapper mapper
) : IRequestHandler<GetProductByIdQuery, Result<ProductDetailDto>>
{
    public async Task<Result<ProductDetailDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new ProductByIdSpec(request.Id, request.IncludeChildren);
        var product = await repo.FirstOrDefaultAsync(spec, cancellationToken);
        if (product is null)
            return Result<ProductDetailDto>.Fail(CatalogErrors.ProductNotFound);

        var dto = mapper.Map<ProductDetailDto>(product);

        if (request.IncludeChildren)
        {
            var brand = await brandRepo.GetByIdAsync(product.BrandId, cancellationToken);
            if (brand is not null)
            {
                dto.BrandName = brand.Name;
                dto.BrandSlug = brand.Slug.Value;
            }

            var category = await categoryRepo.GetByIdAsync(product.CategoryId, cancellationToken);
            if (category is not null)
            {
                dto.CategoryName = category.CategoryName;
                dto.CategorySlug = category.Slug.Value;
            }
        }

        return Result<ProductDetailDto>.Success(dto);
    }
}
