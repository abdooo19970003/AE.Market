using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteCategoryAttribute;

public sealed record DeleteCategoryAttributeCommand(Guid Id) : ICommand;
