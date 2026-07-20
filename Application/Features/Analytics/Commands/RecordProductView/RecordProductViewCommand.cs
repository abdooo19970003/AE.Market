using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Analytics.Commands.RecordProductView;

public sealed record RecordProductViewCommand(Guid ProductId) : ICommand;
