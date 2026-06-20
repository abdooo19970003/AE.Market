using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveBundleItem;

public sealed record RemoveBundleItemCommand(
    Guid ProductId,
    Guid BundleItemId
) : ICommand;
