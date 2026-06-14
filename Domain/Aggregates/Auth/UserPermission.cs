using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Auth
{
    public sealed class UserPermission : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Permission Permission { get; private set; }

        internal UserPermission(Guid id, Guid userId, Permission permission)
            : base(id)
        {
            this.UserId = userId;
            Permission = permission;
        }
        internal static UserPermission Create(Guid id, Guid userId, Permission permission) => new(id, userId, permission);
        private UserPermission() { }

    }
}
