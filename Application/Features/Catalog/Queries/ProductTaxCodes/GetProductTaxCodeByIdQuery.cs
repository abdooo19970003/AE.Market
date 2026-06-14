using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.ProductTaxCodes;

public sealed record GetProductTaxCodeByIdQuery(Guid Id) : IBaseQuery<ProductTaxCodeDto>;
