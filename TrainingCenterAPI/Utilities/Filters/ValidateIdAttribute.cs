using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

/// <summary>
/// Reusable action filter that validates a route "id" parameter is
/// greater than 0, before the controller action runs. Applied via
/// [ValidateId] on any endpoint with an int id in the route
/// (GetById, Update, Delete), across every controller — Course,
/// Student, Instructor, Enrollment alike.
///
/// Principle: Single Responsibility + DRY (Don't Repeat Yourself).
/// Route-id validation logic is written once here, instead of
/// repeating the same "if (id < 1) return BadRequest(...)" check
/// inside every action of every controller.
///
/// Example — before using this filter, every id-based action needed:
/// 
///     if (id < 1)
///     {
///         return BadRequest($"Not accepted ID {id}");
///     }
///
/// After applying [ValidateId] above the action, that check is
/// removed entirely — the filter intercepts invalid ids (like -1 or 0)
/// automatically, before the method body ever runs:
/// 
///     [ValidateId]
///     public async Task&lt;IActionResult&gt; Delete(int id)
///     {
///         var success = await _courseService.DeleteCourseAsync(id);
///         if (!success)
///         {
///             return NotFound($"Course with ID {id} not found.");
///         }
///         return NoContent();
///     }
/// </summary>

namespace TrainingCenterAPI.Utilities.Filters
{
    public class ValidateIdAttribute : ActionFilterAttribute
    {
        private readonly string[] _parameterNames;

        public ValidateIdAttribute(params string[] parameterNames)
        {
            _parameterNames = parameterNames.Length > 0 ? parameterNames : new[] { "id" };
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var name in _parameterNames)
            {
                if (context.ActionArguments.TryGetValue(name, out var value) && value is int idValue && idValue < 1)
                {
                    context.Result = new BadRequestObjectResult(new { error = $"ID must be greater than 0. Received: {idValue}" });
                    return;
                }
            }
        }
    }
}