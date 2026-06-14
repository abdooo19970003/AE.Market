using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IRepository<Product> repo,
    IMapper mapper
) : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var productType = (ProductType)Enum.Parse(typeof(ProductType), request.ProductType);
        var product = Product.Create(
            Guid.NewGuid(),
            request.Name,
            request.Slug,
            request.Sku,
            request.CategoryId,
            productType,
            request.Details
        );

        product.UpdateBrand(request.BrandId);
        product.UpdateTaxCode(request.TaxCodeId);
        product.SetAllowBackOrder(request.AllowBackOrder, request.BackOrderLimit);
        product.SetShortDescription(request.ShortDescription);
        product.SetLongDescription(request.LongDescription);

        await repo.AddAsync(product, cancellationToken);

        var dto = mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(dto);
    }
}
