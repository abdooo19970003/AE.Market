using AE.Market.Domain.Aggregates.Auth.Events;
using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth
{
    public class User : BaseEntity, IAggregateRoot
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

        private User(Guid id)
            : base(id) { }

        public static User Register(Guid id, string email, string passwordHash)
        {
            var hash = PasswordHash.FromHashedString(passwordHash);
            var emailAddress = EmailAddress.Create(email);
            var user = new User(id, emailAddress, hash);
            user.AddDominEvent(new UserRegisteredEvent(user.Id));
            return user;
        }

        // Permissions Managment

        private readonly List<UserPermission> _permissions = [];
        public IReadOnlyCollection<UserPermission> Permissions => _permissions;

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
        private readonly List<RefreshToken> _refreshTokens = [];
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

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
                AddDominEvent(new RefreshTokenReusedEvent(Id, oldToken));
                //return null;
                throw Exceptions.Auth.ReplayAttackDetected;
            }
            if (token.IsExpired || token.IsDeleted)
                throw Exceptions.Auth.TokenExpiredOrRevoked;
            token.MarkConsumed();
            return AddRefreshToken(newToken, expiry);
        }
    }
}
