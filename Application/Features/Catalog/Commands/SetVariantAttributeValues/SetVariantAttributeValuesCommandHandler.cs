using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.SetVariantAttributeValues;

internal sealed class SetVariantAttributeValuesCommandHandler(
    IRepository<Product> repo
) : IRequestHandler<SetVariantAttributeValuesCommand, Result>
{
    public async Task<Result> Handle(SetVariantAttributeValuesCommand request, CancellationToken cancellationToken)
    {
        var spec = new ProductByIdSpec(request.ProductId, includeChildren: true);
        var product = await repo.GetBySpecWithTrackingAsync(spec, cancellationToken);
        if (product is null)
            return Result.Fail(CatalogErrors.ProductNotFound);

        foreach (var attr in request.AttributeValues)
        {
            product.SetVariantAttributeValue(
                request.VariantId,
                Guid.NewGuid(),
                attr.AttributeId,
                attr.InputType,
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
