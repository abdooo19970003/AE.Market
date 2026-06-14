using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Slug,
    string? Details,
    string? ShortDescription,
    string? LongDescription,
    Guid CategoryId,
    Guid BrandId,
    Guid TaxCodeId,
    string ProductType,
    bool AllowBackOrder,
    int? BackOrderLimit,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords
) : ICommand<ProductDto>;
