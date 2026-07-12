using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.ProductRelations;

public sealed record GetProductRelationByIdQuery(Guid Id) : IBaseQuery<ProductRelationDto>;
