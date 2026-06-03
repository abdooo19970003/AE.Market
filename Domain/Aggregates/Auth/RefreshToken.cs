using AE.Market.Domain.Aggregates.Auth.Events;
using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; private set; }
        public DateTime ExpiryTime { get; private set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiryTime;
        public DateTime? ConsumedAt { get; private set; }
        public Guid UserId { get; private set; }
        public User? User { get; private set; }

        private RefreshToken(Guid id) : base(id)
        {
            
        }
        internal RefreshToken(Guid id,Guid userId, string token, DateTime expiry)
            : base(id)
        {
            UserId = userId;
            Token = token;
            ExpiryTime = expiry;
        }
        internal void MarkConsumed() { 
            ConsumedAt = DateTime.UtcNow;
        }
      
    }
}
