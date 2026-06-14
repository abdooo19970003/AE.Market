using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    string Slug,
    string? Description,
    Guid? ParentId,
    string? ImageUrl,
    int SortOrder
) : ICommand<CategoryDto>;
