using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Auth.DTOs;

namespace AE.Market.Application.Features.Auth.Commands.CreateProfile;

public sealed record CreateUserProfileCommand(
    string FirstName,
    string? LastName
) : ICommand<UserProfileDto>;
