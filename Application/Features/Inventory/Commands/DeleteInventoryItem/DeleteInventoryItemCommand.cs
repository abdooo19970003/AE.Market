using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Inventory.Commands.DeleteInventoryItem;

public sealed record DeleteInventoryItemCommand(Guid VariantId) : ICommand;
