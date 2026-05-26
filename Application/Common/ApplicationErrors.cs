using AE.Market.Domain.Common;

namespace AE.Market.Application.Common
{
    public static class ApplicationErrors
    {
        public static Error ApplicationError(string message) => new("Application",message);
        public static Error ValidationError => new("Application.Validation", "Validation Failed");
    }
}
