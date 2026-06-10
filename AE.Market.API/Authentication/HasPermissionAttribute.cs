using AE.Market.Domain.Aggregates.Auth;
using Microsoft.AspNetCore.Authorization;

namespace AE.Market.API.Authentication
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HasPermissionAttribute(Permission permission) : Attribute
    {
        public Permission Permission { get; } = permission;
    }


}
