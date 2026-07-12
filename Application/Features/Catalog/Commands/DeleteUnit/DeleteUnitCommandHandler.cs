using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Common.Abstracts;
using DomainUnit = AE.Market.Domain.Aggregates.Catalog.Units.Unit;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteUnit;

internal sealed class DeleteUnitCommandHandler(
    IRepository<DomainUnit> repo
) : IRequestHandler<DeleteUnitCommand, Result>
{
    private static readonly Error UnitNotFound = new("Catalog.Unit.NotFound", "The specified unit was not found.");

    public async Task<Result> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (unit is null)
            return Result.Fail(UnitNotFound);
        if (unit is null)
            return Result.Fail(UnitNotFound);

        unit.Delete();

        return Result.Success();
    }
}
