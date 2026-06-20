using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.AssignAttributeToGroup;

public sealed record AssignAttributeToGroupCommand(
    Guid AttributeId,
    Guid? AttributeGroupId
) : ICommand<CategoryAttributeDto>;
