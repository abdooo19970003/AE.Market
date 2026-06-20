using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteUnit;

public sealed record DeleteUnitCommand(Guid Id) : ICommand;
