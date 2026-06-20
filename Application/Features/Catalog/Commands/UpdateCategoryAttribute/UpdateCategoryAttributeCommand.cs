using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateCategoryAttribute;

public sealed record UpdateCategoryAttributeCommand(
    Guid Id,
    string AttributeName,
    bool IsRequired,
    bool IsFilterable,
    int SortOrder
) : ICommand<CategoryAttributeDto>;
