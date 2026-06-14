using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.CreateBrand;

public sealed record CreateBrandCommand(
    string Name,
    string Slug,
    string? ShortDescription,
    string? LongDescription,
    string? LogoUrl,
    string? WebsiteUrl,
    int SortOrder
) : ICommand<BrandDto>;
