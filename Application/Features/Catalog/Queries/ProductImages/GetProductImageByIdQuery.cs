using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.ProductImages;

public sealed record GetProductImageByIdQuery(Guid Id) : IBaseQuery<ProductImageDto>;
