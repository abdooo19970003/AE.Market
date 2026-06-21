using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.CategoryAttributes;

internal sealed class GetAttributeGroupsByCategoryQueryHandler(
    IReadRepository<AttributeGroup> repo,
    IReadRepository<Category> categoryRepo,
    IMapper mapper
) : IRequestHandler<GetAttributeGroupsByCategoryQuery, Result<List<AttributeGroupDto>>>
{
    public async Task<Result<List<AttributeGroupDto>>> Handle(GetAttributeGroupsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var category = await categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result<List<AttributeGroupDto>>.Fail(CatalogErrors.CategoryNotFound);

        var spec = new AttributeGroupsByCategorySpec(request.CategoryId);
        var groups = await repo.ListWithSpecAsync(spec, cancellationToken);
        var dtos = mapper.Map<List<AttributeGroupDto>>(groups);

        return Result<List<AttributeGroupDto>>.Success(dtos);
    }
}
