using Serilog.Context;

namespace AE.Market.API.Middlewares
{
    public sealed class RequestLoggingMiddleware(RequestDelegate next)
    {

        public Task InvokeAsync(HttpContext context) {

            using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier)) return next(context);
        }
    }
}
