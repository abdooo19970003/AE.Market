using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand;
