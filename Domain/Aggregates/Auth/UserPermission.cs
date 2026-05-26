namespace AE.Market.Domain.Aggregates.Auth
{
    public class UserPermission
    {
        public Guid UserId { get; private set; }
        public User? User { get; private set; }
        public Permission Permission { get; private set; }

        internal UserPermission(Guid UserId, Permission permission)
        {
            this.UserId = UserId;
            Permission = permission;
        }


    }
}
