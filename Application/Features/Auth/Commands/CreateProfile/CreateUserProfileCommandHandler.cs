using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Common;
using MediatR;

namespace AE.Market.Application.Features.Auth.Commands.CreateProfile;

internal sealed class CreateUserProfileCommandHandler(
    IRepository<User> userRepo,
    IRepository<UserProfile> profileRepo,
    ICurrentUser currentUser,
    IMapper mapper
) : IRequestHandler<CreateUserProfileCommand, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(
        CreateUserProfileCommand request,
        CancellationToken cancellationToken
    )
    {
        var user = await userRepo.GetByIdWithTrackingAsync(currentUser.UserId, cancellationToken);
        if (user is null)
            return Result<UserProfileDto>.Fail(AuthErrors.UserNotFound);

        if (user.Profile is not null)
            return Result<UserProfileDto>.Fail(ProfileErrors.ProfileAlreadyExists);

        user.CreateProfile(Guid.NewGuid(), request.FirstName, request.LastName);
        await profileRepo.AddAsync(user.Profile!, cancellationToken);

        return Result<UserProfileDto>.Success(mapper.Map<UserProfileDto>(user.Profile!));
    }
}
