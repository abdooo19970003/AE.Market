using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.SetProductAttributeValue;

internal sealed class SetProductAttributeValueCommandHandler(
    IRepository<Product> repo
) : IRequestHandler<SetProductAttributeValueCommand, Result>
{
    public async Task<Result> Handle(SetProductAttributeValueCommand request, CancellationToken cancellationToken)
    {
        var spec = new ProductByIdSpec(request.ProductId, includeChildren: true);
        var product = await repo.GetBySpecWithTrackingAsync(spec, cancellationToken);
        if (product is null)
            return Result.Fail(CatalogErrors.ProductNotFound);

        product.SetAttributeValue(
            Guid.NewGuid(),
            request.AttributeId,
            request.InputType,
            request.IsVariantDefiner,
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
