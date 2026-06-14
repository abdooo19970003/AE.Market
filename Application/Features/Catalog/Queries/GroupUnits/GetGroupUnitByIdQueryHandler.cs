using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Units;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.GroupUnits;

internal sealed class GetGroupUnitByIdQueryHandler(
    IReadRepository<GroupUnit> repo,
    IMapper mapper
) : IRequestHandler<GetGroupUnitByIdQuery, Result<GroupUnitDto>>
{
    public async Task<Result<GroupUnitDto>> Handle(GetGroupUnitByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new GroupUnitByIdSpec(request.Id, request.IncludeUnits);
        var groupUnit = await repo.FirstOrDefaultAsync(spec, cancellationToken);
        if (groupUnit is null)
            return Result<GroupUnitDto>.Fail(CatalogErrors.GroupUnitNotFound);

        var dto = mapper.Map<GroupUnitDto>(groupUnit);
        return Result<GroupUnitDto>.Success(dto);
    }
}
