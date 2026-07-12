using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.ProductTaxCodes;

public sealed record GetProductTaxCodesListQuery : IBaseQuery<List<ProductTaxCodeDto>>;
