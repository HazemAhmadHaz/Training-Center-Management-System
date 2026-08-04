using Microsoft.AspNetCore.Authorization;

namespace TrainingCenterAPI.Services.Security;

public class InstructorOwnershipRequirement : IAuthorizationRequirement
{
}