using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Units;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateGroupUnit;

internal sealed class UpdateGroupUnitCommandHandler(
    IRepository<GroupUnit> repo,
    IMapper mapper
) : IRequestHandler<UpdateGroupUnitCommand, Result<GroupUnitDto>>
{
    public async Task<Result<GroupUnitDto>> Handle(UpdateGroupUnitCommand request, CancellationToken cancellationToken)
    {
        var groupUnit = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (groupUnit is null)
            return Result<GroupUnitDto>.Fail(CatalogErrors.GroupUnitNotFound);

        groupUnit.Rename(request.Name);

        var dto = mapper.Map<GroupUnitDto>(groupUnit);
        return Result<GroupUnitDto>.Success(dto);
    }
}
