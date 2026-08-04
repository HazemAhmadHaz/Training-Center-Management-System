using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Services.Interfaces;
using TrainingCenterAPI.Utilities.Filters;

namespace TrainingCenterAPI.Controllers;

[ApiController]
[Route("api/Students/{studentId}/profile")]
[Authorize(Policy = "OwnStudentOrAdmin")]
public class StudentProfilesController : ControllerBase
{
    private readonly IStudentProfileService _profileService;

    public StudentProfilesController(IStudentProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet(Name = "GetStudentProfile")]
    [ValidateId("studentId")]
    
    public async Task<ActionResult<StudentProfileDto>> Get(int studentId) => Ok(await _profileService.GetProfileAsync(studentId));

    [HttpPost(Name = "CreateStudentProfile")]
    [ValidateId("studentId")]
    
    public async Task<ActionResult<StudentProfileDto>> Create(int studentId, [FromBody] CreateStudentProfileDto dto)
    {
        var profile = await _profileService.CreateProfileAsync(studentId, dto);
        return CreatedAtRoute("GetStudentProfile", new { studentId }, profile);
    }

    [HttpPut(Name = "UpdateStudentProfile")]
    [ValidateId("studentId")]
    
    public async Task<IActionResult> Update(int studentId, [FromBody] UpdateStudentProfileDto dto)
    {
        await _profileService.UpdateProfileAsync(studentId, dto);
        return NoContent();
    }

    [HttpDelete(Name = "DeleteStudentProfile")]
    [ValidateId("studentId")]
    
    public async Task<IActionResult> Delete(int studentId)
    {
        await _profileService.DeleteProfileAsync(studentId);
        return NoContent();
    }
}
