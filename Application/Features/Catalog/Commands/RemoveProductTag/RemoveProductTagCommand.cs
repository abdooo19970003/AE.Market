using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveProductTag;

public sealed record RemoveProductTagCommand(
    Guid ProductId,
    string Slug
) : ICommand;
