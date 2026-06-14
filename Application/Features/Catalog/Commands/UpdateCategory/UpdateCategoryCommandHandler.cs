using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateCategory;

internal sealed class UpdateCategoryCommandHandler(
    IRepository<Category> repo,
    IMapper mapper
) : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (category is null)
            return Result<CategoryDto>.Fail(CatalogErrors.CategoryNotFound);

        category.UpdateDetails(request.Name, request.Description, request.ImageUrl, request.SortOrder);

        var dto = mapper.Map<CategoryDto>(category);
        return Result<CategoryDto>.Success(dto);
    }
}
