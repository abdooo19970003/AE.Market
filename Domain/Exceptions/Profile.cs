namespace AE.Market.Domain.Exceptions
{
    public static class Profile
    {
        public static readonly DomainException InvalidImageUrl = new(
            "Domain.Profile.ProfileImage.Invalid",
            "Invalid Image url"
        );

        public static readonly DomainException PhoneNumberTooShort = new(
            "Domain.Profile.PhoneNumber.TooShort",
            "Phone should have at least 7 digits"
        );
    }
}
