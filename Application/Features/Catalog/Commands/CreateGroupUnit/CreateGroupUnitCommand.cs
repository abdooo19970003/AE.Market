using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.CreateGroupUnit;

public sealed record CreateGroupUnitCommand(string Name) : ICommand<GroupUnitDto>;
