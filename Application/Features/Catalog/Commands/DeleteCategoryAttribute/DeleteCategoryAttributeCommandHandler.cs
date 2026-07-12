using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteCategoryAttribute;

internal sealed class DeleteCategoryAttributeCommandHandler(
    IRepository<CategoryAttribute> repo
) : IRequestHandler<DeleteCategoryAttributeCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryAttributeCommand request, CancellationToken cancellationToken)
    {
        var attribute = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (attribute is null)
            return Result.Fail(CatalogErrors.AttributeNotFound);

        attribute.Delete();

        return Result.Success();
    }
}
