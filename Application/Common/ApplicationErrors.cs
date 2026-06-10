using AE.Market.Domain.Common;

namespace AE.Market.Application.Common
{
    public static class ApplicationErrors
    {
        public static Error ApplicationError(string? code, string message) => new($"Application.{code}", message);
        public static Error ValidationError => new("Application.Validation", "Validation Failed");
        public static Error NotFoundError(string resource) => new("Application.NotFound", $"{resource} was not found.");
        public static Error ConflictError(string detail) => new("Application.Conflict", detail);
    }
}
