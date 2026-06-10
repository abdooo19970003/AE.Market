namespace AE.Market.Application.Features.Auth.DTOs
{
    public sealed record UserDetailsDto 
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = [];
        public List<RefreshTokenDto> RefreshTokens { get; set; } = [];
        public UserProfileDto? Profile { get; set; }
    }
    public sealed record RefreshTokenDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }

    }
}
