using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Auth.Commands.DisableUser;

public sealed record DisableUserCommand(Guid UserId) : ICommand;
