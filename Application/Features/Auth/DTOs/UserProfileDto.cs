namespace AE.Market.Application.Features.Auth.DTOs;

public sealed record UserProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public List<AddressDto> Addresses { get; set; } = [];
    public string? ProfileImage { get; set; }
}
