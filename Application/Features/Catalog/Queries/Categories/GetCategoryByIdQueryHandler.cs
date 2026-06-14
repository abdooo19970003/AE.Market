using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.Categories;

internal sealed class GetCategoryByIdQueryHandler(
    IReadRepository<Category> repo,
    IMapper mapper
) : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new CategoryByIdSpec(request.Id, request.IncludeChildren);
        var category = await repo.FirstOrDefaultAsync(spec, cancellationToken);
        if (category is null)
            return Result<CategoryDto>.Fail(CatalogErrors.CategoryNotFound);

        var dto = mapper.Map<CategoryDto>(category);
        return Result<CategoryDto>.Success(dto);
    }
}
