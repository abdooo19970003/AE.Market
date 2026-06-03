namespace AE.Market.API.Options
{
    public class JwtOptions
    {
        public static string Section => "Jwt";
        public string Secret { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public double ExpirationInMinutes { get; set; }
    }
}
