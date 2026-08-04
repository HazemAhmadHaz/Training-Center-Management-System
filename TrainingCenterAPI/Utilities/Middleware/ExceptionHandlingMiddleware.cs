using System.Net;
using System.Text.Json;
using TrainingCenterAPI.Utilities.Exceptions;

namespace TrainingCenterAPI.Utilities.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            IHostEnvironment env,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _env = env;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                await WriteErrorResponseAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (BusinessRuleException ex)
            {
                await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (UnauthorizedException ex)
            {
                await WriteErrorResponseAsync(
                    context,
                    HttpStatusCode.Unauthorized,
                    ex.Message);
            }

            catch (ForbiddenException ex)
            {
                await WriteErrorResponseAsync(
                    context,
                    HttpStatusCode.Forbidden,
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception occurred on {Path}",
                    context.Request.Path);

                if (!context.Response.HasStarted)
                {
                    await WriteErrorResponseAsync(
                        context,
                        HttpStatusCode.InternalServerError,
                        _env.IsDevelopment()
                            ? ex.Message
                            : "An unexpected error occurred.");
                }
            }
        }

        private static async Task WriteErrorResponseAsync(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            var result = JsonSerializer.Serialize(new { error = message });
            await context.Response.WriteAsync(result);
        }
    }
}