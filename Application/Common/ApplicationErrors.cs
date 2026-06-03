using AE.Market.Domain.Common;

namespace AE.Market.Application.Common
{
    public static class ApplicationErrors
    {
        public static Error ApplicationError(string? code,string message) => new($"Application.{code}",message);
        public static Error ValidationError => new("Application.Validation", "Validation Failed");
    }
}
