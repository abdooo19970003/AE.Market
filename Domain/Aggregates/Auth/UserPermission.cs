using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth
{
    public class UserPermission : BaseEntity
    {
        public Guid UserId { get; private set; }
        public User? User { get; private set; }
        public Permission Permission { get; private set; }

        internal UserPermission(Guid id, Guid UserId, Permission permission)
            : base(id)
        {
            this.UserId = UserId;
            Permission = permission;
        }


    }
}
