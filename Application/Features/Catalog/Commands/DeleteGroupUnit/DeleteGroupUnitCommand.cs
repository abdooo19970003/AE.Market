using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteGroupUnit;

public sealed record DeleteGroupUnitCommand(Guid Id) : ICommand;
