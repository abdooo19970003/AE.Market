using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Auth.DTOs;

namespace AE.Market.Application.Features.Auth.Commands.UpdateProfile;

public sealed record UpdateUserProfileCommand(
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? ProfileImageUrl,
    List<AddressDto>? Addresses
) : ICommand<UserProfileDto>;
