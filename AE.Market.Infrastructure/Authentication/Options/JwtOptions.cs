namespace AE.Market.Infrastructure.Authentication.Options
{
    public class JwtOptions
    {
        public static string Section => "Jwt";
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public double ExpirationInMinutes { get; set; }
    }
}
