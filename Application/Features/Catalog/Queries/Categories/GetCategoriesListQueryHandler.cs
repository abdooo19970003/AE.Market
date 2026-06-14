using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.Categories;

internal sealed class GetCategoriesListQueryHandler(
    IReadRepository<Category> repo,
    IMapper mapper
) : IRequestHandler<GetCategoriesListQuery, Result<List<CategoryDto>>>
{
    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesListQuery request, CancellationToken cancellationToken)
    {
        var categories = await repo.ListAsync(cancellationToken);
        var dtos = mapper.Map<List<CategoryDto>>(categories);
        return Result<List<CategoryDto>>.Success(dtos);
    }
}
