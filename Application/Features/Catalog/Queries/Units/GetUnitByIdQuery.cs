using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.Units;

public sealed record GetUnitByIdQuery(Guid Id) : IBaseQuery<UnitDto>;
