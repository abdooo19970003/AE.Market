using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateAttributeGroup;

public sealed record UpdateAttributeGroupCommand(
    Guid Id,
    string GroupName,
    string? Slug,
    int SortOrder
) : ICommand<AttributeGroupDto>;
