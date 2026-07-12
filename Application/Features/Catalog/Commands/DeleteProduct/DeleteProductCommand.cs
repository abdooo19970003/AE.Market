using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : ICommand;
