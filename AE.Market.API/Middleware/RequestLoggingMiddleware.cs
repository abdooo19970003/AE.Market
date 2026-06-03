using Serilog.Context;

namespace AE.Market.API.Middleware
{
    public class RequestLoggingMiddleware(RequestDelegate next)
    {

        public Task InvokeAsync(HttpContext context) {

            using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier)) return next(context);
        }
    }
}
