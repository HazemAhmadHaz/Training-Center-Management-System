using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Services.Interfaces;
using TrainingCenterAPI.Utilities.Filters;

namespace TrainingCenterAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstructorsController : ControllerBase
{
    private readonly IInstructorService _instructorService;

    public InstructorsController(IInstructorService instructorService)
    {
        _instructorService = instructorService;
    }

    //
    //
    //

    [HttpGet(Name = "GetAllInstructors")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var (instructors, totalCount) =
            await _instructorService.GetAllInstructorsAsync(
                isActive,
                page,
                pageSize);

        return Ok(new
        {
            totalCount,
            page,
            pageSize,
            items = instructors
        });
    }

    //
    //
    //

    [HttpGet("{id}", Name = "GetInstructorById")]
    [ValidateId]
    [Authorize(Policy = "OwnInstructorOrAdmin")]
    public async Task<ActionResult<InstructorDto>> GetById(int id)
    {
        return Ok(
            await _instructorService.GetInstructorByIdAsync(id));
    }

    //
    //
    //

    [HttpPost(Name = "CreateInstructor")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<InstructorDto>> Create(
        [FromBody] CreateInstructorDto dto)
    {
        var instructor =
            await _instructorService.CreateInstructorAsync(dto);

        return CreatedAtRoute(
            "GetInstructorById",
            new { id = instructor.InstructorId },
            instructor);
    }

    //
    //
    //

    [HttpPut("{id}", Name = "UpdateInstructor")]
    [ValidateId]
    [Authorize(Policy = "OwnInstructorOrAdmin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateInstructorDto dto)
    {
        await _instructorService.UpdateInstructorAsync(id, dto);

        return NoContent();
    }

    //
    //
    //

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}", Name = "DeleteInstructor")]
    [ValidateId]
    public async Task<IActionResult> Delete(int id)
    {
        await _instructorService.DeleteInstructorAsync(id);

        return NoContent();
    }
}