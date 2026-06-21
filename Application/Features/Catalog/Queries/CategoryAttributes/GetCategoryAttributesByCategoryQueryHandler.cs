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

internal sealed class GetCategoryAttributesByCategoryQueryHandler(
    IReadRepository<CategoryAttribute> repo,
    IReadRepository<Category> categoryRepo,
    IMapper mapper
) : IRequestHandler<GetCategoryAttributesByCategoryQuery, Result<List<CategoryAttributeDto>>>
{
    public async Task<Result<List<CategoryAttributeDto>>> Handle(GetCategoryAttributesByCategoryQuery request, CancellationToken cancellationToken)
    {
        var category = await categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result<List<CategoryAttributeDto>>.Fail(CatalogErrors.CategoryNotFound);

        var spec = new CategoryAttributesByCategorySpec(request.CategoryId);
        var attributes = await repo.ListWithSpecAsync(spec, cancellationToken);
        var dtos = mapper.Map<List<CategoryAttributeDto>>(attributes);

        return Result<List<CategoryAttributeDto>>.Success(dtos);
    }
}
