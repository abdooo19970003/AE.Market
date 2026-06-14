using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Auth.Errors
{
    public static class ProfileErrors
    {
        public static readonly Error ProfileAlreadyExists = new(
            "Domain.Profile.AlreadyExists",
            "User profile already exists."
        );
        public static readonly Error ProfileNotFound = new(
            "Domain.Profile.NotFound",
            "User profile not found. Create one first."
        );
    }
}
