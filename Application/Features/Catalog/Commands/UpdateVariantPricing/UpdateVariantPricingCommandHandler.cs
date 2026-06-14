using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateVariantPricing;

internal sealed class UpdateVariantPricingCommandHandler(
    IRepository<Product> repo,
    IMapper mapper
) : IRequestHandler<UpdateVariantPricingCommand, Result<VariantDto>>
{
    public async Task<Result<VariantDto>> Handle(UpdateVariantPricingCommand request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdWithTrackingAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<VariantDto>.Fail(CatalogErrors.ProductNotFound);

        var variant = product.Variants.FirstOrDefault(v => v.Id == request.VariantId);
        if (variant is null)
            return Result<VariantDto>.Fail(CatalogErrors.VariantNotFound);

        product.SetVariantSalePrice(request.VariantId, request.SalePrice);
        repo.Update(product);

        var dto = mapper.Map<VariantDto>(variant);
        return Result<VariantDto>.Success(dto);
    }
}
