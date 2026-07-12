using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.AddProductTag;

public sealed record AddProductTagCommand(
    Guid ProductId,
    string Name,
    string Slug
) : ICommand<TagDto>;
