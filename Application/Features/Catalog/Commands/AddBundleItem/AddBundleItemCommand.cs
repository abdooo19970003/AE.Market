using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.AddBundleItem;

public sealed record AddBundleItemCommand(
    Guid ProductId,
    Guid ItemId,
    int Quantity
) : ICommand<BundleItemDto>;
