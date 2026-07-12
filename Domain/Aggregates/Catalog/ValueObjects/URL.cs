using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.ValueObjects
{
    public sealed record URL : IValueObject
    {
        public string Value { get; }

    private URL(string value) => Value = value;

    public static URL CreateAbsolute(string absoluteUrl)
    {
        if (string.IsNullOrWhiteSpace(absoluteUrl))
            throw new ArgumentException("Absolute URL cannot be empty.", nameof(absoluteUrl));

        if (!Uri.TryCreate(absoluteUrl, UriKind.Absolute, out var uri))
            throw new ArgumentException($"Invalid absolute URL: '{absoluteUrl}'.", nameof(absoluteUrl));

        var scheme = uri.Scheme.ToLowerInvariant();
        if (scheme != "http" && scheme != "https")
            throw new ArgumentException($"URL scheme '{scheme}' is not supported. Only HTTP and HTTPS are allowed.", nameof(absoluteUrl));

        return new URL(absoluteUrl.TrimEnd('/'));
    }

    public static URL Create(params string[] segments)
        {
            if (segments == null || segments.Length == 0)
                throw new ArgumentException("At least one URL segment is required.");

            // 1. Sanitize: Remove nulls, trim whitespace, and filter illegal characters
            var sanitizedSegments = segments
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim('/', ' '))
                .ToList();

            if (sanitizedSegments.Count == 0)
                throw new ArgumentException(
                    "URL segments cannot be empty or contain only invalid characters."
                );

            // 2. Validate: Ensure segments don't contain URL-breaking characters
            // Example: Checking for query string characters which shouldn't be in a base slug
            foreach (var segment in sanitizedSegments)
            {
                if (segment.Contains('?') || segment.Contains('#') || segment.Contains('&'))
                    throw new ArgumentException(
                        $"URL segment '{segment}' contains illegal characters."
                    );
            }

            // 3. Build: Join and ensure absolute path format
            var path = string.Join("/", sanitizedSegments);

            return new URL($"/{path}");
        }
    }
}
