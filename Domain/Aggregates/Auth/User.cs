using AE.Market.Domain.Aggregates.Auth.Events;
using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth
{
    public sealed class User : BaseEntity, IAggregateRoot
    {
        public EmailAddress Email { get; private set; }
        public PasswordHash PasswordHash { get; private set; }
        public UserProfile? Profile { get; private set; }

        private User(Guid id, EmailAddress emailAddress, PasswordHash hash)
            : base(id)
        {
            Email = emailAddress;
            PasswordHash = hash;
        }

        // Paramaterless constructor
        private User() { }

        // static factory method
        public static User Register(Guid id, string email, string passwordHash)
        {
            var hash = PasswordHash.FromHashedString(passwordHash);
            var emailAddress = EmailAddress.Create(email);
            var user = new User(id, emailAddress, hash);
            user.AddDomainEvent(new UserRegisteredDomainEvent(user.Id));
            return user;
        }

        // Permissions Managment
        private readonly List<UserPermission> _permissions = new List<UserPermission>();
        public IReadOnlyCollection<UserPermission> Permissions => _permissions.AsReadOnly();

        public UserPermission AddPermission(Permission permission)
        {
            UserPermission NewPermission = new(Guid.NewGuid(), this.Id, permission);
            _permissions.Add(NewPermission);
            UpdateLastModified();
            return NewPermission;
        }

        public void RemovePermission(UserPermission permission)
        {
            _permissions.Remove(permission);
            UpdateLastModified();
        }

        public void AddPermissions(IEnumerable<Permission> permissions)
        {
            foreach (var permission in permissions)
                AddPermission(permission);
        }

        public void RemovePermissions(IEnumerable<UserPermission> permissions)
        {
            foreach (var permission in permissions)
                RemovePermission(permission);
        }

        // Refresh Tokens
        private readonly List<RefreshToken> _refreshTokens = new List<RefreshToken>();
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        public RefreshToken AddRefreshToken(string token, TimeSpan expiry)
        {
            if (_refreshTokens.Count(t => !t.IsExpired) >= 5)
            {
                var oldest = _refreshTokens
                    .Where(t => !t.IsExpired && !t.IsDeleted)
                    .MinBy(t => t.CreatedAt);
                if (oldest != null)
                {
                    oldest.Delete();
                    _refreshTokens.Remove(oldest);
                }
            }
            var newToken = new RefreshToken(Guid.NewGuid(), Id, token, DateTime.UtcNow + expiry);
            _refreshTokens.Add(newToken);
            return newToken;
        }

        public void RevokeRefreshTokens()
        {
            foreach (RefreshToken refreshToken in _refreshTokens)
            {
                refreshToken.Delete();
            }
            _refreshTokens.Clear();
        }

        public RefreshToken RotateRefreshToken(string oldToken, string newToken, TimeSpan expiry)
        {
            var token = _refreshTokens.FirstOrDefault(t => t.Token == oldToken);
            Guard.AgainstNull(token, nameof(token));

            // Replay Attack Detaction
            if (token?.ConsumedAt is not null)
            {
                // Force Logout
                RevokeRefreshTokens();
                // Rise Domain Event for notification and analytics ...
                AddDomainEvent(new RefreshTokenReusedDomainEvent(Id, oldToken));
                //return null;
                throw Exceptions.Auth.ReplayAttackDetected;
            }
            if (token.IsExpired || token.IsDeleted)
                throw Exceptions.Auth.TokenExpiredOrRevoked;
            token.MarkConsumed();
            return AddRefreshToken(newToken, expiry);
        }

        // User Profile Manegment
        public void CreateProfile(Guid profileId, string firstName, string? lastName)
        {
            if (Profile is not null)
                throw new InvalidOperationException("Profile already exists.");

            Profile = UserProfile.Create(profileId, this.Id, firstName, lastName);
            UpdateLastModified();
        }

        public void UpdateProfilePhone(string phoneNumber)
        {
            GuardAgainstMissingProfile();
            if (string.IsNullOrEmpty(phoneNumber))
                Profile!.RemovePhoneNumber();
            else
                Profile!.SetPhoneNumber(phoneNumber);
            UpdateLastModified();
        }

        public void UpdateProfileAddress(string city, string country, string? addressLine)
        {
            GuardAgainstMissingProfile();
            Profile!.SetAddress(city, country, addressLine);
            UpdateLastModified();
        }

        public void RemoveProfileAddress()
        {
            GuardAgainstMissingProfile();
            Profile!.RemoveAddress();
            UpdateLastModified();
        }

        public void UpdateProfileImage(string url)
        {
            GuardAgainstMissingProfile();
            if (string.IsNullOrEmpty(url))
                Profile!.SetProfileImage(url);
            else
                Profile!.SetProfileImage(url);
            UpdateLastModified();
        }

        private void GuardAgainstMissingProfile()
        {
            if (Profile is null)
                throw new InvalidOperationException("User profile is not initialized.");
        }
    }
}
