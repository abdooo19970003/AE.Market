using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.CreateCategoryAttribute;

public sealed record CreateCategoryAttributeCommand(
    string AttributeName,
    string InputType,
    Guid CategoryId,
    bool IsRequired,
    bool IsFilterable,
    string? Slug,
    int SortOrder,
    Guid? DefaultUnitId,
    Guid? AllowedGroupUnitId
) : ICommand<CategoryAttributeDto>;
