using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.Categories;

public sealed record GetCategoriesListQuery : IBaseQuery<List<CategoryDto>>;
