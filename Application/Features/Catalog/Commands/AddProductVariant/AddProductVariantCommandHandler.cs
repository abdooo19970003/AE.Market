using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.AddProductVariant;

internal sealed class AddProductVariantCommandHandler(
    IRepository<Product> repo,
    IMapper mapper
) : IRequestHandler<AddProductVariantCommand, Result<VariantDto>>
{
    public async Task<Result<VariantDto>> Handle(AddProductVariantCommand request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdWithTrackingAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<VariantDto>.Fail(CatalogErrors.ProductNotFound);

        var variant = product.AddVariant(Guid.NewGuid(), request.Name, request.Sku);
        repo.Update(product);

        var dto = mapper.Map<VariantDto>(variant);
        return Result<VariantDto>.Success(dto);
    }
}
