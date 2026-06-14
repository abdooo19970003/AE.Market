using AE.Market.Domain.Common;
using AE.Market.Domain.Common.Abstracts;
using System.Text.RegularExpressions;

namespace AE.Market.Domain.Aggregates.Catalog.ValueObjects;

public sealed record Slug : IValueObject
{
    public string Value { get; }

    private static readonly Regex SlugPattern = new(
        @"^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.Compiled
    );

    private Slug(string value) => Value = value;

    public static Slug Create(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(Slug));
        var slug = value.Trim().ToLowerInvariant().Replace(" ", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        Guard.AgainstStringTooShort(slug, nameof(Slug), 2);

        return new Slug(slug);
    }

    public static Slug? CreateNullable(string? value)
    {
        return value is null ? null : Create(value);
    }

    public static Slug From(string slug) => Slug.Create(slug);

    public override string ToString() => Value;

    public static implicit operator string(Slug slug) => slug.Value;
}
