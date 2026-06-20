using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.AddProductVariant;

internal sealed class AddProductVariantCommandHandler(
    IRepository<Product> repo,
    IRepository<ProductVariant> variantRepo,
    IMapper mapper
) : IRequestHandler<AddProductVariantCommand, Result<VariantDto>>
{
    public async Task<Result<VariantDto>> Handle(AddProductVariantCommand request, CancellationToken cancellationToken)
    {
        var spec = new ProductByIdSpec(request.ProductId, includeChildren: true);
        var product = await repo.GetBySpecWithTrackingAsync(spec, cancellationToken);
        if (product is null)
            return Result<VariantDto>.Fail(CatalogErrors.ProductNotFound);

        var variant = product.AddVariant(Guid.NewGuid(), request.Name, request.Sku);
        await variantRepo.AddAsync(variant, cancellationToken);

        var dto = mapper.Map<VariantDto>(variant);
        return Result<VariantDto>.Success(dto);
    }
}
