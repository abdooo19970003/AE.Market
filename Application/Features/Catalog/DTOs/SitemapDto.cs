namespace AE.Market.Application.Features.Catalog.DTOs;

public sealed record SitemapDto(
    IReadOnlyList<SitemapUrlDto> Urls
);

public sealed record SitemapUrlDto(
    string Loc,
    DateTime? LastMod,
    string ChangeFreq,
    double Priority
);
