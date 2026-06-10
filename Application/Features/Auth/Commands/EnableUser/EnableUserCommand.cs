using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Auth.Commands.EnableUser;

public sealed record EnableUserCommand(Guid UserId) : ICommand;
