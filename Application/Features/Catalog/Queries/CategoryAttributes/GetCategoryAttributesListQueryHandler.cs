using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.Specifications;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.CategoryAttributes;

internal sealed class GetCategoryAttributesListQueryHandler(
    IReadRepository<CategoryAttribute> repo,
    IMapper mapper
) : IRequestHandler<GetCategoryAttributesListQuery, Result<PaginatedList<CategoryAttributeDto>>>
{
    public async Task<Result<PaginatedList<CategoryAttributeDto>>> Handle(GetCategoryAttributesListQuery request, CancellationToken cancellationToken)
    {
        var spec = new BaseSpecification<CategoryAttribute>();
        var totalCount = await repo.CountAsync(spec, cancellationToken);
        spec.SetPagination((request.Page - 1) * request.PageSize, request.PageSize);

        var items = await repo.ListWithSpecAsync(spec, cancellationToken);
        var dtos = mapper.Map<List<CategoryAttributeDto>>(items);

        var result = new PaginatedList<CategoryAttributeDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PaginatedList<CategoryAttributeDto>>.Success(result);
    }
}
