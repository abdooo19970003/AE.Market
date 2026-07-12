using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteAttributeGroup;

public sealed record DeleteAttributeGroupCommand(Guid Id) : ICommand;
