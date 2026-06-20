using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.SetProductAttributeValues;

internal sealed class SetProductAttributeValuesCommandHandler(
    IRepository<Product> repo
) : IRequestHandler<SetProductAttributeValuesCommand, Result>
{
    public async Task<Result> Handle(SetProductAttributeValuesCommand request, CancellationToken cancellationToken)
    {
        var spec = new ProductByIdSpec(request.ProductId, includeChildren: true);
        var product = await repo.GetBySpecWithTrackingAsync(spec, cancellationToken);
        if (product is null)
            return Result.Fail(CatalogErrors.ProductNotFound);

        foreach (var attr in request.AttributeValues)
        {
            product.SetAttributeValue(
                Guid.NewGuid(),
                attr.AttributeId,
                attr.InputType,
                attr.IsVariantDefiner,
                attr.ValueText,
                attr.ValueInteger,
                attr.ValueDecimal,
                attr.ValueBoolean,
                attr.ValueDateTime,
                attr.OptionId
            );
        }

        return Result.Success();
    }
}
