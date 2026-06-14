using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.CreateCategory;

internal sealed class CreateCategoryCommandHandler(
    IRepository<Category> repo,
    IMapper mapper
) : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.Create(
            Guid.NewGuid(),
            request.Name,
            request.Slug,
            request.Description,
            request.ParentId,
            request.ImageUrl,
            request.SortOrder
        );

        await repo.AddAsync(category, cancellationToken);

        var dto = mapper.Map<CategoryDto>(category);
        return Result<CategoryDto>.Success(dto);
    }
}
