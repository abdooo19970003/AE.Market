using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateBrand;

internal sealed class UpdateBrandCommandHandler(
    IRepository<Brand> repo,
    IMapper mapper
) : IRequestHandler<UpdateBrandCommand, Result<BrandDto>>
{
    public async Task<Result<BrandDto>> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (brand is null)
            return Result<BrandDto>.Fail(CatalogErrors.BrandNotFound);

        URL? websiteUrl = request.WebsiteUrl is not null
            ? URL.CreateAbsolute(request.WebsiteUrl)
            : null;

        brand.UpdateDetails(request.Name, request.ShortDescription, request.LongDescription, request.LogoUrl, websiteUrl, request.SortOrder);

        var dto = mapper.Map<BrandDto>(brand);
        return Result<BrandDto>.Success(dto);
    }
}
