using Microsoft.Extensions.Logging;

namespace TrainingCenterAPI.Utilities.Middleware
{
    /// <summary>
    /// Middleware responsible for generating and managing a unique correlation ID
    /// for each incoming HTTP request.
    ///
    /// The correlation ID allows a single request to be traced across different
    /// application layers, including controllers, services, repositories, and
    /// logs. This makes debugging and monitoring easier.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";

        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(
            RequestDelegate next,
            ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId =
                context.Request.Headers.TryGetValue(
                    CorrelationIdHeader,
                    out var existingId)
                ? existingId.ToString()
                : Guid.NewGuid().ToString();

            // Add ID to response
            context.Response.Headers[CorrelationIdHeader] =
                correlationId;


            // Add ID to logs
            using (_logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId
                }))
            {
                await _next(context);
            }
        }
    }
}