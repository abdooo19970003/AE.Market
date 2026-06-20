using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveProductImage;

public sealed record RemoveProductImageCommand(
    Guid ProductId,
    Guid ImageId
) : ICommand;
