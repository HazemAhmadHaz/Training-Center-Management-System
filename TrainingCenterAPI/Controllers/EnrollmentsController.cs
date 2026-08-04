using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Services.Interfaces;
using TrainingCenterAPI.Utilities.Filters;

namespace TrainingCenterAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(
        IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }


    // =========================
    // Get All Enrollments
    // Admin + Instructor
    // =========================

    [HttpGet(Name = "GetAllEnrollments")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? studentId,
        [FromQuery] int? courseId,
        [FromQuery] EnrollmentStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var (enrollments, totalCount) =
            await _enrollmentService.GetAllEnrollmentsAsync(
                studentId,
                courseId,
                status,
                page,
                pageSize);

        return Ok(new
        {
            totalCount,
            page,
            pageSize,
            items = enrollments
        });
    }


    // =========================
    // Get Enrollment By Id
    // Student Owner + Admin
    // =========================

    [HttpGet("{id}", Name = "GetEnrollmentById")]
    [Authorize(Policy = "OwnEnrollmentOrAdmin")]
    [ValidateId]
    public async Task<ActionResult<EnrollmentDto>> GetById(int id)
    {
        var enrollment =
            await _enrollmentService.GetEnrollmentByIdAsync(id);

        return Ok(enrollment);
    }


    // =========================
    // Create Enrollment
    // Student + Admin
    // =========================

    [HttpPost(Name = "CreateEnrollment")]
    [Authorize(Roles = "Admin,Student")]
    public async Task<ActionResult<EnrollmentDto>> Create(
        [FromBody] CreateEnrollmentDto dto)
    {
        var enrollment =
            await _enrollmentService.CreateEnrollmentAsync(dto);

        return CreatedAtRoute(
            "GetEnrollmentById",
            new
            {
                id = enrollment.EnrollmentId
            },
            enrollment);
    }


    // =========================
    // Update Enrollment
    // Admin + Instructor
    // =========================

    [HttpPut("{id}", Name = "UpdateEnrollment")]
    [Authorize(Roles = "Admin,Student")]
    [ValidateId]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateEnrollmentDto dto)
    {
        await _enrollmentService.UpdateEnrollmentAsync(id, dto);

        return NoContent();
    }


    // =========================
    // Delete Enrollment
    // Admin
    // =========================

    [HttpDelete("{id}", Name = "DeleteEnrollment")]
    [Authorize(Roles = "Admin")]
    [ValidateId]
    public async Task<IActionResult> Delete(int id)
    {
        await _enrollmentService.DeleteEnrollmentAsync(id);

        return NoContent();
    }
}