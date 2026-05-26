namespace AE.Market.Domain.Exceptions
{
    public static class Profile
    {
        public static readonly DomainException InvalidPhoneNumber = new(
            "Domain.Profile.Phone.Invalid",
            "Invalid phone number, phone number should contain only 0-9 numbers e.g:05554443322"
        );

        public static readonly DomainException InvalidImageUrl = new(
            "Domain.Profile.ProfileImage.Invalid",
            "Invalid Image url"
        );

        public static readonly DomainException InvalidName = new(
            "Domain.Profile.Name.Invalid",
            "Name should contain letters only"
        );

        public static readonly DomainException NameTooShort = new(
            "Domain.Profile.Name.TooShort",
            "Name should have at least 3 letters"
        );

        public static readonly DomainException PhoneNumberTooShort = new(
            "Domain.Profile.PhoneNumber.TooShort",
            "Phone should have at least 7 digits"
        );

        public static readonly DomainException FailedParseAddress = new(
            "Domain.Profile.Address.ParseFailed",
            "Address should be in format 'country,city,[addressline]'"
        );

        public static readonly DomainException InvalidCity = new(
            "Domain.Profile.Address.City.Invalid",
            "City is required."
        );

        public static readonly DomainException InvalidCountry = new(
            "Domain.Profile.Address.Country.Invalid",
            "Country is required."
        );
    }

}
