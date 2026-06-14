using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.Brands;

public sealed record GetBrandsListQuery : IBaseQuery<List<BrandDto>>;
