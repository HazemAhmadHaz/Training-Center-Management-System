using Microsoft.AspNetCore.Http;

namespace TrainingCenterAPI.Utilities.Middleware
{
    /// <summary>
    /// Middleware that adds security-related HTTP response headers to the API.
    /// 
    /// These headers provide additional protection against common browser-based
    /// attacks and unsafe browser behavior, such as clickjacking and MIME-type
    /// sniffing.
    /// 
    /// The middleware does not handle authentication or authorization.
    /// JWT authentication, roles, and authorization policies determine who can
    /// access an endpoint, while security headers tell the browser how it should
    /// handle the API response.
    /// 
    /// Request pipeline:
    ///     Request
    ///        ↓
    ///     SecurityHeadersMiddleware
    ///        ↓
    ///     ExceptionHandlingMiddleware
    ///        ↓
    ///     Authentication
    ///        ↓
    ///     Authorization
    ///        ↓
    ///     Controller
    ///        ↓
    ///     Service / Repository
    /// 
    /// This follows the Single Responsibility Principle by keeping browser-level
    /// security header configuration separate from controllers and business logic.
    /// 
    /// This header:
    /// X-Frame-Options: DENY
    /// basically tells the browser:
    /// "Don't allow this page to be displayed inside an iframe."
    /// 
    /// Another example
    /// X-Content-Type-Options: nosniff
    /// tells the browser:
    /// "Don't try to guess the content type. Use the type that the server specified."
    /// </summary>

    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

            context.Response.Headers["X-Frame-Options"] = "DENY";

            context.Response.Headers["Referrer-Policy"] =
                "strict-origin-when-cross-origin";

            context.Response.Headers["Permissions-Policy"] =
                "camera=(), microphone=(), geolocation=()";

            await _next(context);
        }
    }
}