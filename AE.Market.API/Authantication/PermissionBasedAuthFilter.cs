using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace AE.Market.API.Authantication
{
    public class PermissionBasedAuthFilter : IAsyncAuthorizationFilter
    {
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var attribute = (HasPermissionAttribute?) context.ActionDescriptor.EndpointMetadata.FirstOrDefault(x => x is HasPermissionAttribute);
            if (attribute != null) {
                var claimsIdentity = context.HttpContext.User.Identity as ClaimsIdentity;
                Claim? permissionsClaim =  claimsIdentity?.Claims.FirstOrDefault(x => x.Type == "Permissions");
                if(permissionsClaim is null) {
                    // dont allow 
                    context.Result = new ForbidResult(JwtBearerDefaults.AuthenticationScheme);

                }
                var grantedPermessions = permissionsClaim?.Value.Split(',');
                if (!grantedPermessions.Contains(attribute.Permission.ToString()))
                {
                    // dont  allow
                    context.Result = new ForbidResult(JwtBearerDefaults.AuthenticationScheme);
                }
            }
            return Task.CompletedTask;
        }
    }
}
