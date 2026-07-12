namespace AE.Market.Domain.Common.Abstracts
{
    public abstract class BaseEntity
    {
        // Basic properties
        public Guid Id { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime LastModified { get; private set; } = DateTime.UtcNow;
        public bool IsDeleted { get; private set; } = false;

        // Main Constructor
        protected BaseEntity(Guid id)
        {
            Id = id;
        }

        // Parameterless Constructor for ORM
        protected BaseEntity() { }

        // Domain Events
        private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected internal void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        // Basic Methods
        protected void UpdateLastModified()
        {
            LastModified = DateTime.UtcNow;
        }

        public virtual void Delete()
        {
            if (!IsDeleted)
            {
                IsDeleted = true;
                UpdateLastModified();
                AddDomainEvent(new EntityDeletedDomainEvent(this));
            }
        }

        public virtual void Restore()
        {
            if (IsDeleted)
            {
                IsDeleted = false;
                UpdateLastModified();
                AddDomainEvent(new EntityRestoredDomainEvent(this));
            }
        }

        // Override HashCode
        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        // Override Equals
        public override bool Equals(object? obj)
        {
            if (obj is null
                || obj is not BaseEntity other
                || this.GetType() != other.GetType()
                ) return false;
            return this.Id == other.Id;
        }

        // Operators
        public static bool operator ==(BaseEntity? a, BaseEntity? b)
        {
            if (a is null)
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

    public sealed record EntityDeletedDomainEvent(BaseEntity Entity) : IDomainEvent;
    public sealed record EntityRestoredDomainEvent(BaseEntity Entity) : IDomainEvent;
}
