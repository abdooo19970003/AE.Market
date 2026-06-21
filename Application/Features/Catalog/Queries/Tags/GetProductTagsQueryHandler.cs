using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.Tags;

internal sealed class GetProductTagsQueryHandler(
    IReadRepository<Product> repo,
    IMapper mapper
) : IRequestHandler<GetProductTagsQuery, Result<List<TagDto>>>
{
    public async Task<Result<List<TagDto>>> Handle(GetProductTagsQuery request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<List<TagDto>>.Fail(CatalogErrors.ProductNotFound);

        var dtos = mapper.Map<List<TagDto>>(product.Tags.ToList());
        return Result<List<TagDto>>.Success(dtos);
    }
}
