using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveProductRelation;

public sealed record RemoveProductRelationCommand(
    Guid ProductId,
    Guid RelationId
) : ICommand;
