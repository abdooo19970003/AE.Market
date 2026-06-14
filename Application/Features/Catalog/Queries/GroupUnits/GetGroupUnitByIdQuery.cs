using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.GroupUnits;

public sealed record GetGroupUnitByIdQuery(Guid Id, bool IncludeUnits = false) : IBaseQuery<GroupUnitDto>;
