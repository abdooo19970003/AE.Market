using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Auth.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId) : ICommand;
