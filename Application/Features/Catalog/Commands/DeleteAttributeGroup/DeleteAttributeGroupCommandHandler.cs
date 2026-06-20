using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteAttributeGroup;

internal sealed class DeleteAttributeGroupCommandHandler(
    IRepository<AttributeGroup> repo
) : IRequestHandler<DeleteAttributeGroupCommand, Result>
{
    public async Task<Result> Handle(DeleteAttributeGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (group is null)
            return Result.Fail(CatalogErrors.AttributeGroupNotFound);

        group.Delete();

        return Result.Success();
    }
}
