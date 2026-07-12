using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateGroupUnit;

public sealed record UpdateGroupUnitCommand(Guid Id, string Name) : ICommand<GroupUnitDto>;
