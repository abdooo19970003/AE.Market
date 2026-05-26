using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth.Errors
{
    public static class ProfileErrors
        {
            public static readonly Error InvalidPhoneNumber = new(
                "Domain.Profile.Phone.Invalid",
                "Invalid phone number, phone number should containe only 0-9 numbers e.g:05554443322"
                );
            public static readonly Error InvalidImageUrl = new(
                "Domain.Profile.ProfileImage.Invalid",
                "Invalid Image url"
                );
            public static readonly Error InvalidName = new(
                "Domain.Profile.Name.Invalid",
                "Name should contain letters only"
                );
            public static readonly Error NameTooShort = new(
               "Domain.Profile.Name.TooShort",
               "Name should have at least 3 letter"
               );
            public static readonly Error PhoneNumberTooShort = new(
              "Domain.Profile.PhoneNumber.TooShort",
              "Phone should have at least 7 letter"
              );

            public static Error FailedParseAddress = new(
              "Domain.Profile.Address.ParseFailed",
              "Address should be in format 'country,city,[addressline]'"
              );
            public static Error InvalidCity = new(
              "Domain.Profile.Address.City.Invalid",
              "City is required."
              );
            public static Error InvalidCountry = new(
              "Domain.Profile.Address.Country.Invalid",
              "Country is required."
              );
        }
    
}
