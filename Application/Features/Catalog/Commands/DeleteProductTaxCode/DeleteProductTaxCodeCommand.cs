using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteProductTaxCode;

public sealed record DeleteProductTaxCodeCommand(Guid Id) : ICommand;
