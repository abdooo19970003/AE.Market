using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.CategoryAttributes;

internal sealed class GetCategoryAttributeByIdQueryHandler(
    IReadRepository<CategoryAttribute> repo,
    IMapper mapper
) : IRequestHandler<GetCategoryAttributeByIdQuery, Result<CategoryAttributeDto>>
{
    public async Task<Result<CategoryAttributeDto>> Handle(GetCategoryAttributeByIdQuery request, CancellationToken cancellationToken)
    {
        var attribute = await repo.FirstOrDefaultAsync(new CategoryAttributeByIdSpec(request.Id), cancellationToken);
        if (attribute is null)
            return Result<CategoryAttributeDto>.Fail(CatalogErrors.AttributeNotFound);

        var dto = mapper.Map<CategoryAttributeDto>(attribute);
        return Result<CategoryAttributeDto>.Success(dto);
    }
}
