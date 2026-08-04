using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TrainingCenterAPI.Services.Interfaces;

namespace TrainingCenterAPI.Services.Security;

public class InstructorOwnershipHandler
    : AuthorizationHandler<InstructorOwnershipRequirement>
{
    private readonly IInstructorService _instructorService;

    public InstructorOwnershipHandler(
        IInstructorService instructorService)
    {
        _instructorService = instructorService;
    }


    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        InstructorOwnershipRequirement requirement)
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


        if (!int.TryParse(idClaim, out int instructorId))
            return;


        var instructor =
            await _instructorService.GetInstructorByIdAsync(instructorId);


        if (instructor == null)
            return;


        var routeId =
            context.Resource is HttpContext httpContext
            ? httpContext.Request.RouteValues["id"]?.ToString()
            : null;


        if (routeId == instructor.InstructorId.ToString())
        {
            context.Succeed(requirement);
        }
    }
}