using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateBrand;

public sealed record UpdateBrandCommand(
    Guid Id,
    string Name,
    string? ShortDescription,
    string? LongDescription,
    string? LogoUrl,
    string? WebsiteUrl,
    int SortOrder
) : ICommand<BrandDto>;
