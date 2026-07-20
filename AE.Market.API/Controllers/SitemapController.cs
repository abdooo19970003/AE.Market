using System.Text;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Queries.Sitemap;
using AE.Market.API.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class SitemapController(IMediator mediator) : ControllerBase
{
    [HttpGet("sitemap.xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> GetSitemap(CancellationToken ct)
    {
        var result = await mediator.Send(new GetSitemapQuery(), ct);
        if (!result.IsSuccess)
            return result.ToNotFoundActionResult();

        var xml = GenerateXml(result.Value);
        return Content(xml, "application/xml", Encoding.UTF8);
    }

    private static string GenerateXml(SitemapDto sitemap)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        foreach (var url in sitemap.Urls)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{url.Loc}</loc>");
            if (url.LastMod.HasValue)
                sb.AppendLine($"    <lastmod>{url.LastMod.Value:yyyy-MM-dd}</lastmod>");
            sb.AppendLine($"    <changefreq>{url.ChangeFreq}</changefreq>");
            sb.AppendLine($"    <priority>{url.Priority}</priority>");
            sb.AppendLine("  </url>");
        }

        sb.AppendLine("</urlset>");
        return sb.ToString();
    }
}
