using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.Specifications;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.Categories;

internal sealed class GetCategoriesListQueryHandler(
    IReadRepository<Category> repo,
    IMapper mapper
) : IRequestHandler<GetCategoriesListQuery, Result<PaginatedList<CategoryDto>>>
{
    public async Task<Result<PaginatedList<CategoryDto>>> Handle(GetCategoriesListQuery request, CancellationToken cancellationToken)
    {
        var spec = new BaseSpecification<Category>();
        var totalCount = await repo.CountAsync(spec, cancellationToken);
        spec.SetPagination((request.Page - 1) * request.PageSize, request.PageSize);

        var items = await repo.ListWithSpecAsync(spec, cancellationToken);
        var dtos = mapper.Map<List<CategoryDto>>(items);

        var result = new PaginatedList<CategoryDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PaginatedList<CategoryDto>>.Success(result);
    }
}
