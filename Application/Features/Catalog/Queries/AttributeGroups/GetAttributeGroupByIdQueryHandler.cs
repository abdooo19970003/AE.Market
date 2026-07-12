using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.AttributeGroups;

internal sealed class GetAttributeGroupByIdQueryHandler(
    IReadRepository<AttributeGroup> repo,
    IMapper mapper
) : IRequestHandler<GetAttributeGroupByIdQuery, Result<AttributeGroupDto>>
{
    public async Task<Result<AttributeGroupDto>> Handle(GetAttributeGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var group = await repo.FirstOrDefaultAsync(new AttributeGroupByIdSpec(request.Id), cancellationToken);
        if (group is null)
            return Result<AttributeGroupDto>.Fail(CatalogErrors.AttributeGroupNotFound);

        var dto = mapper.Map<AttributeGroupDto>(group);
        return Result<AttributeGroupDto>.Success(dto);
    }
}
