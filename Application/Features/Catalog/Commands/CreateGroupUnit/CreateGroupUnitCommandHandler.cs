using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Units;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.CreateGroupUnit;

internal sealed class CreateGroupUnitCommandHandler(
    IRepository<GroupUnit> repo,
    IMapper mapper
) : IRequestHandler<CreateGroupUnitCommand, Result<GroupUnitDto>>
{
    public async Task<Result<GroupUnitDto>> Handle(CreateGroupUnitCommand request, CancellationToken cancellationToken)
    {
        var groupUnit = GroupUnit.Create(Guid.NewGuid(), request.Name);
        await repo.AddAsync(groupUnit, cancellationToken);

        var dto = mapper.Map<GroupUnitDto>(groupUnit);
        return Result<GroupUnitDto>.Success(dto);
    }
}
