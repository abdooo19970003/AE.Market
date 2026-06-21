using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.CategoryAttributes;

public sealed record GetAttributeGroupsByCategoryQuery(Guid CategoryId) : IBaseQuery<List<AttributeGroupDto>>;
