using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateProduct;

internal sealed class UpdateProductCommandHandler(
    IRepository<Product> repo,
    IMapper mapper
) : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (product is null)
            return Result<ProductDto>.Fail(CatalogErrors.ProductNotFound);

        var productType = (ProductType)Enum.Parse(typeof(ProductType), request.ProductType);
        product.UpdateDetails(request.Name, request.Details, request.MetaTitle, request.MetaDescription, request.MetaKeywords);
        product.UpdateProductType(productType);
        product.UpdateSlug(request.Slug);
        product.ChangeCategory(request.CategoryId);
        product.UpdateBrand(request.BrandId);
        product.UpdateTaxCode(request.TaxCodeId);
        product.SetAllowBackOrder(request.AllowBackOrder, request.BackOrderLimit);
        product.SetShortDescription(request.ShortDescription);
        product.SetLongDescription(request.LongDescription);

        repo.Update(product);

        var dto = mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(dto);
    }
}
