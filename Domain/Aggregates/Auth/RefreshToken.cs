using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Auth
{
    public sealed class RefreshToken : BaseEntity
    {
        public string TokenHash { get; private set; }
        public DateTime ExpiryTime { get; private set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiryTime;
        public DateTime? ConsumedAt { get; private set; }
        public Guid UserId { get; private set; }

        private RefreshToken() { }

        internal static RefreshToken Create(Guid id, Guid userId, string tokenHash, DateTime expiry)
        {
            return new(id, userId, tokenHash, expiry);
        }

        private RefreshToken(Guid id, Guid userId, string tokenHash, DateTime expiry)
            : base(id)
        {
            UserId = userId;
            TokenHash = tokenHash;
            ExpiryTime = expiry;
        }

        internal void MarkConsumed()
        {
            ConsumedAt = DateTime.UtcNow;
        }
    }
}
