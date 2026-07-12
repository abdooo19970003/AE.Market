using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Inventory.Commands.SetQuantity;

public sealed record SetQuantityCommand(Guid VariantId, int Quantity) : ICommand;
