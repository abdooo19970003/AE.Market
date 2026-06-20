using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.SetVariantAttributeValue;

internal sealed class SetVariantAttributeValueCommandHandler(
    IRepository<Product> repo
) : IRequestHandler<SetVariantAttributeValueCommand, Result>
{
    public async Task<Result> Handle(SetVariantAttributeValueCommand request, CancellationToken cancellationToken)
    {
        var spec = new ProductByIdSpec(request.ProductId, includeChildren: true);
        var product = await repo.GetBySpecWithTrackingAsync(spec, cancellationToken);
        if (product is null)
            return Result.Fail(CatalogErrors.ProductNotFound);

        product.SetVariantAttributeValue(
            request.VariantId,
            Guid.NewGuid(),
            request.AttributeId,
            request.InputType,
            request.ValueText,
            request.ValueInteger,
            request.ValueDecimal,
            request.ValueBoolean,
            request.ValueDateTime,
            request.OptionId
        );

        return Result.Success();
    }
}
