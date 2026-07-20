namespace AE.Market.API.Configuration;

public sealed class CorsOptions
{
    public string[] AdminOrigins { get; set; } = [];
    public string[] PublicOrigins { get; set; } = [];
    public string[] DefaultOrigins { get; set; } = [];
}
