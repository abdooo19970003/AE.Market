using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.Enums;

namespace AE.Market.Domain.Common.ValueObjects;

public record Address : IValueObject
{
    public string Country { get; init; }
    public string City { get; init; }
    public string? ZipCode { get; init; }
    public string? AddressLine { get; init; }
    public string? Label { get; init; }
    public bool IsPrimary { get; init; }
    public AddressType Type { get; init; }

    private Address(
        string country,
        string city,
        string? zipCode,
        string? addressLine,
        string? label,
        bool isPrimary,
        AddressType type)
    {
        Country = country;
        City = city;
        ZipCode = zipCode;
        AddressLine = addressLine;
        Label = label;
        IsPrimary = isPrimary;
        Type = type;
    }

    public static Address Create(
        string country,
        string city,
        string? zipCode = null,
        string? addressLine = null,
        string? label = null,
        bool isPrimary = false,
        AddressType type = AddressType.Residence)
    {
        Guard.AgainstNullOrWhiteSpace(city, "Address.City");
        Guard.AgainstNullOrWhiteSpace(country, "Address.Country");
        return new Address(country, city, zipCode, addressLine, label, isPrimary, type);
    }

    public Address WithLabel(string label) => this with { Label = label };
    public Address MarkPrimary() => this with { IsPrimary = true };
    public Address ClearPrimary() => this with { IsPrimary = false };
    public Address ChangeType(AddressType type) => this with { Type = type };

    public override string ToString() =>
        string.Join(", ", new[] { Country, City, ZipCode, AddressLine }.Where(x => x is not null));
}
