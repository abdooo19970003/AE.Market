using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Auth.Commands.UpdateProfile;

internal sealed class UpdateUserProfileCommandHandler(
    IRepository<User> userRepo,
    ICurrentUser currentUser,
    IMapper mapper
) : IRequestHandler<UpdateUserProfileCommand, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(
        UpdateUserProfileCommand request,
        CancellationToken cancellationToken
    )
    {
        var user = await userRepo.GetByIdWithTrackingAsync(currentUser.UserId, cancellationToken);
        if (user is null)
            return Result<UserProfileDto>.Fail(AuthErrors.UserNotFound);

        if (user.Profile is null)
            return Result<UserProfileDto>.Fail(ProfileErrors.ProfileNotFound);

        if (!string.IsNullOrEmpty(request.FirstName))
            user.UpdateProfileNames(request.FirstName, request.LastName);

        if (request.PhoneNumber is not null)
            user.UpdateProfilePhone(request.PhoneNumber);

        if (request.Addresses is not null)
        {
            user.ClearProfileAddresses();
            foreach (var addr in request.Addresses)
            {
                user.AddProfileAddress(
                    addr.Country,
                    addr.City,
                    addr.AddressLine,
                    addr.Label,
                    addr.IsPrimary,
                    addr.Type);
            }
        }

        if (request.ProfileImageUrl is not null)
            user.UpdateProfileImage(request.ProfileImageUrl);

        return Result<UserProfileDto>.Success(mapper.Map<UserProfileDto>(user.Profile!));
    }
}
