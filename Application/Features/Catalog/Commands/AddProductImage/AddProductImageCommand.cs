using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.AddProductImage;

public sealed record AddProductImageCommand(
    Guid ProductId,
    string Url,
    string? AltText,
    bool IsPrimary,
    int SortOrder
) : ICommand<ProductImageDto>;
