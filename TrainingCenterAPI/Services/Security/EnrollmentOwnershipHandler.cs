using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TrainingCenterAPI.Services.Interfaces;

namespace TrainingCenterAPI.Services.Security;

public class EnrollmentOwnershipHandler
    : AuthorizationHandler<EnrollmentOwnershipRequirement>
{
    private readonly IEnrollmentService _enrollmentService;


    public EnrollmentOwnershipHandler(
        IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }


    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EnrollmentOwnershipRequirement requirement)
    {
        // Admin bypass
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }


        var studentIdClaim =
            context.User.FindFirstValue(
                ClaimTypes.NameIdentifier);


        if (studentIdClaim == null ||
           !int.TryParse(studentIdClaim, out int studentId))
        {
            return;
        }


        var routeId =
            context.Resource is HttpContext httpContext
            ? httpContext.Request.RouteValues["id"]?.ToString()
            : null;


        if (routeId == null ||
           !int.TryParse(routeId, out int enrollmentId))
        {
            return;
        }


        var enrollment =
            await _enrollmentService
                .GetByIdAsync(enrollmentId);


        if (enrollment == null)
            return;


        if (enrollment.StudentId == studentId)
        {
            context.Succeed(requirement);
        }
    }
}