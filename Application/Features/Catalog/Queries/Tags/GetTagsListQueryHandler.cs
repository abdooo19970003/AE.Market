using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.Specifications;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.Tags;

internal sealed class GetTagsListQueryHandler(
    IReadRepository<Tag> repo,
    IMapper mapper
) : IRequestHandler<GetTagsListQuery, Result<PaginatedList<TagDto>>>
{
    public async Task<Result<PaginatedList<TagDto>>> Handle(GetTagsListQuery request, CancellationToken cancellationToken)
    {
        var spec = new BaseSpecification<Tag>();
        var totalCount = await repo.CountAsync(spec, cancellationToken);
        spec.SetPagination((request.Page - 1) * request.PageSize, request.PageSize);

        var items = await repo.ListWithSpecAsync(spec, cancellationToken);
        var dtos = mapper.Map<List<TagDto>>(items);

        var result = new PaginatedList<TagDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PaginatedList<TagDto>>.Success(result);
    }
}
