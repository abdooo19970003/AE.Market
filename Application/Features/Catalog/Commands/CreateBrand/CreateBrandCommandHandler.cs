using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.CreateBrand;

internal sealed class CreateBrandCommandHandler(
    IRepository<Brand> repo,
    IMapper mapper
) : IRequestHandler<CreateBrandCommand, Result<BrandDto>>
{
    public async Task<Result<BrandDto>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        URL? websiteUrl = request.WebsiteUrl is not null
            ? URL.CreateAbsolute(request.WebsiteUrl)
            : null;

        var brand = Brand.Create(
            Guid.NewGuid(),
            request.Name,
            request.Slug,
            request.ShortDescription,
            request.LongDescription,
            request.LogoUrl,
            websiteUrl,
            request.SortOrder
        );

        await repo.AddAsync(brand, cancellationToken);

        var dto = mapper.Map<BrandDto>(brand);
        return Result<BrandDto>.Success(dto);
    }
}
