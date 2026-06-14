using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Units;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteGroupUnit;

internal sealed class DeleteGroupUnitCommandHandler(
    IRepository<GroupUnit> repo
) : IRequestHandler<DeleteGroupUnitCommand, Result>
{
    public async Task<Result> Handle(DeleteGroupUnitCommand request, CancellationToken cancellationToken)
    {
        var groupUnit = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (groupUnit is null)
            return Result.Fail(CatalogErrors.GroupUnitNotFound);

        groupUnit.Delete();

        return Result.Success();
    }
}
