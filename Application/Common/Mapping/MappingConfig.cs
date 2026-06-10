using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Domain.Aggregates.Auth;
using Mapster;

namespace AE.Market.Application.Common.Mapping;

public static class MappingConfig
{
    public static TypeAdapterConfig Configure()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.NewConfig<UserProfile, UserProfileDto>()
            .Map(dest => dest.City, src => src.Address == null ? null : src.Address.City)
            .Map(dest => dest.Country, src => src.Address == null ? null : src.Address.Country)
            .Map(dest => dest.AddressLine, src => src.Address == null ? null : src.Address.AddressLine);

        config.NewConfig<User, UserDetailsDto>()
            .Map(dest => dest.Permissions, src => src.Permissions.Select(x => x.Permission.ToString()).ToList())
            .Map(dest => dest.RefreshTokens, src => src.RefreshTokens.Adapt<List<RefreshTokenDto>>())
            .Map(dest => dest.Profile, src => src.Profile == null ? null : src.Profile.Adapt<UserProfileDto>());

        config.NewConfig<RefreshToken, RefreshTokenDto>()
            .Ignore(dest => dest.Token);

        config.NewConfig<User, UsersListItemDto>();

        config.Compile();
        return config;
    }
}
