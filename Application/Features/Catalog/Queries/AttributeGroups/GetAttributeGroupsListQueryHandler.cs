using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.Specifications;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.AttributeGroups;

internal sealed class GetAttributeGroupsListQueryHandler(
    IReadRepository<AttributeGroup> repo,
    IMapper mapper
) : IRequestHandler<GetAttributeGroupsListQuery, Result<PaginatedList<AttributeGroupDto>>>
{
    public async Task<Result<PaginatedList<AttributeGroupDto>>> Handle(GetAttributeGroupsListQuery request, CancellationToken cancellationToken)
    {
        var spec = new BaseSpecification<AttributeGroup>();
        var totalCount = await repo.CountAsync(spec, cancellationToken);
        spec.SetPagination((request.Page - 1) * request.PageSize, request.PageSize);

        var items = await repo.ListWithSpecAsync(spec, cancellationToken);
        var dtos = mapper.Map<List<AttributeGroupDto>>(items);

        var result = new PaginatedList<AttributeGroupDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PaginatedList<AttributeGroupDto>>.Success(result);
    }
}
