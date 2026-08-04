/// <summary>
/// Represents a broken business rule, such as using an email that already
/// belongs to another student or making an invalid status change.
///
/// The middleware converts this exception into a 400 Bad Request response.
/// This keeps business-rule checks in the Service layer and prevents
/// controllers from repeating error-response code.
/// </summary>

/// <summary>
/// Service
///   ↓
/// throws BusinessRuleException / NotFoundException with a message
///   ↓
/// ExceptionHandlingMiddleware catches it
///   ↓
/// Middleware chooses the HTTP status code
///   ↓
/// Client receives JSON error message
/// </summary>

namespace TrainingCenterAPI.Utilities.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}