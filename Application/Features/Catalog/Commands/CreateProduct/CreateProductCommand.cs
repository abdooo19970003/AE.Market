using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Slug,
    string Sku,
    string? Details,
    string? ShortDescription,
    string? LongDescription,
    Guid CategoryId,
    Guid BrandId,
    Guid TaxCodeId,
    string ProductType,
    bool AllowBackOrder,
    int? BackOrderLimit,
    IReadOnlyCollection<AttributeValueDto>? AttributeValues = null
) : ICommand<ProductDto>;
