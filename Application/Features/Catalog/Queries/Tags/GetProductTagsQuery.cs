using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.Tags;

public sealed record GetProductTagsQuery(Guid ProductId) : IBaseQuery<List<TagDto>>;
