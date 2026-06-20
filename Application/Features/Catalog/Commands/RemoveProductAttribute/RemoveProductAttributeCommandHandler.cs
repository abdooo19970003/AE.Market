using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveProductAttribute;

internal sealed class RemoveProductAttributeCommandHandler(
    IRepository<Product> repo
) : IRequestHandler<RemoveProductAttributeCommand, Result>
{
    public async Task<Result> Handle(RemoveProductAttributeCommand request, CancellationToken cancellationToken)
    {
        var spec = new ProductByIdSpec(request.ProductId, includeChildren: true);
        var product = await repo.GetBySpecWithTrackingAsync(spec, cancellationToken);
        if (product is null)
            return Result.Fail(CatalogErrors.ProductNotFound);

        var attributeValue = product.AttributeValues.FirstOrDefault(av => av.Id == request.AttributeValueId);
        if (attributeValue is null)
            return Result.Fail(CatalogErrors.AttributeNotFound);

        product.RemoveAttributeValue(attributeValue);
        return Result.Success();
    }
}
