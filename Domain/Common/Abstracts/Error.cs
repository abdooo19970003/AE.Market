namespace AE.Market.Domain.Common.Abstracts
{
    public record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
    }
}
