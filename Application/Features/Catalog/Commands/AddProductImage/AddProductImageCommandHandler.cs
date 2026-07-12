using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.AddProductImage;

internal sealed class AddProductImageCommandHandler(
    IRepository<Product> repo,
    IMapper mapper
) : IRequestHandler<AddProductImageCommand, Result<ProductImageDto>>
{
    public async Task<Result<ProductImageDto>> Handle(AddProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdWithTrackingAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<ProductImageDto>.Fail(CatalogErrors.ProductNotFound);

        var image = product.AddImage(Guid.NewGuid(), request.Url, request.AltText, request.IsPrimary, request.SortOrder);

        var dto = mapper.Map<ProductImageDto>(image);
        return Result<ProductImageDto>.Success(dto);
    }
}
