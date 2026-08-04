using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TrainingCenterAPI.Services.Interfaces;

namespace TrainingCenterAPI.Services.Security;

public class StudentOwnershipHandler
    : AuthorizationHandler<StudentOwnershipRequirement>
{
    private readonly IStudentService _studentService;

    public StudentOwnershipHandler(
        IStudentService studentService)
    {
        _studentService = studentService;
    }


    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StudentOwnershipRequirement requirement)
    {
        // Admin bypass
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }


        var idClaim =
            context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");


        if (!int.TryParse(idClaim, out int studentId))
            return;


        var student =
            await _studentService.GetStudentByIdAsync(studentId);


        if (student == null)
            return;


        var routeId =
            context.Resource is HttpContext httpContext
            ? httpContext.Request.RouteValues["id"]?.ToString()
            : null;


        if (routeId == student.StudentId.ToString())
        {
            context.Succeed(requirement);
        }
    }
}