using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.DeleteBrand;

public sealed record DeleteBrandCommand(Guid Id) : ICommand;
