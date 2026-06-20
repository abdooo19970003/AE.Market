using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.AddBundleItem;

internal sealed class AddBundleItemCommandHandler(
    IRepository<Product> repo,
    IMapper mapper
) : IRequestHandler<AddBundleItemCommand, Result<BundleItemDto>>
{
    public async Task<Result<BundleItemDto>> Handle(AddBundleItemCommand request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdWithTrackingAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<BundleItemDto>.Fail(CatalogErrors.ProductNotFound);

        var bundleItem = product.AddBundleItem(Guid.NewGuid(), request.ItemId, request.Quantity);

        var dto = mapper.Map<BundleItemDto>(bundleItem);
        return Result<BundleItemDto>.Success(dto);
    }
}
