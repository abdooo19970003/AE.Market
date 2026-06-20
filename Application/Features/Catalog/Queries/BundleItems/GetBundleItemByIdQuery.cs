using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.BundleItems;

public sealed record GetBundleItemByIdQuery(Guid Id) : IBaseQuery<BundleItemDto>;
