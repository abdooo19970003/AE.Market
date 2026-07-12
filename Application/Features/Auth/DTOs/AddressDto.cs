using AE.Market.Domain.Common.Enums;

namespace AE.Market.Application.Features.Auth.DTOs;

public sealed record AddressDto
{
    public string Country { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string? ZipCode { get; init; }
    public string? AddressLine { get; init; }
    public string? Label { get; init; }
    public bool IsPrimary { get; init; }
    public AddressType Type { get; init; }
}
