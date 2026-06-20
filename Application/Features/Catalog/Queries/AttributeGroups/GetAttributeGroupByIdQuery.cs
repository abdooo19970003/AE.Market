using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.AttributeGroups;

public sealed record GetAttributeGroupByIdQuery(Guid Id) : IBaseQuery<AttributeGroupDto>;
