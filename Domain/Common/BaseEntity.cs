namespace AE.Market.Domain.Common
{
    public abstract class BaseEntity
    {
        // Basic properties
        public Guid Id { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime LastModified { get; private set; } = DateTime.UtcNow;
        public bool IsDeleted { get; private set; } = false;

        // Main Constructor
        internal BaseEntity(Guid id)
        {
            Id = id;
        }

        // Parameterless Constructor for ORM
        private BaseEntity() { }

        // Domain Events
        private readonly List<IDomainEvent> _domainEvets = [];
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvets;

        public void AddDominEvent(IDomainEvent domainEvent)
        {
            _domainEvets.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvets.Clear();
        }

        // Basic Methods
        public void UpdateLastModified()
        {
            LastModified = DateTime.UtcNow;
        }

        public void Delete()
        {
            IsDeleted = true;
            UpdateLastModified();
        }
        public void Restore() {
            IsDeleted = false;
            LastModified = DateTime.UtcNow;
        }

        // Override HashCode
        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        // Override Operators 
        public override bool Equals(object? obj)
        {
            if(obj is null 
                || obj is not BaseEntity other
                || this.GetType() != other.GetType()
                ) return false;
            return this.Id == other.Id;
        }

        // Operators 
        public static bool operator ==(BaseEntity? a, BaseEntity? b)
        {
            if(a is null)
            {
                return b is null;
            }
            return a.Equals(b);
        }
        public static bool operator !=(BaseEntity? a, BaseEntity? b)
        {
            return !(a == b);
        }

    }
}
