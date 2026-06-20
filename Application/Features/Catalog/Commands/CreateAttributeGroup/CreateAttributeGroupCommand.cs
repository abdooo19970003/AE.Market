using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.CreateAttributeGroup;

public sealed record CreateAttributeGroupCommand(
    Guid CategoryId,
    string GroupName,
    string? Slug,
    int SortOrder
) : ICommand<AttributeGroupDto>;
