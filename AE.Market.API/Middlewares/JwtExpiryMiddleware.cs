using AE.Market.Application.Common.Interfaces;

namespace AE.Market.API.Middlewares;

public sealed class JwtExpiryMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICurrentUser currentUser)
    {
        if (currentUser.IsAuthenticated && currentUser.IsTokenExpiringSoon())
        {
            context.Response.Headers["X-Token-Expiry"] = "approaching";
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Token expiring soon",
                message = "Please use your refresh token to get a new access token"
            });
            return;
        }

        await next(context);
    }
}
