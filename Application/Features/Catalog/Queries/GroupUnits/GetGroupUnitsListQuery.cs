using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.GroupUnits;

public sealed record GetGroupUnitsListQuery : IBaseQuery<List<GroupUnitDto>>;
