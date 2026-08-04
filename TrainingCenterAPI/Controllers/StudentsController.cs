using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Services.Interfaces;
using TrainingCenterAPI.Utilities.Filters;

namespace TrainingCenterAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet(Name = "GetAllStudents")]
    [Authorize(Roles = "Admin")]
    /// <summary>
    /// [FromQuery] Get the filter values from the API URL.
    /// Example request:
    /// GET /api/Students? status = 0 & page = 1 & pageSize = 10
    /// </summary>

    public async Task<IActionResult> GetAll([FromQuery] StudentStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (students, totalCount) = await _studentService.GetAllStudentsAsync(status, page, pageSize);
        return Ok(new { totalCount, page, pageSize, items = students });
    }

    //
    // why
    //

    [Authorize(Policy = "OwnStudentOrAdmin")]
    [HttpGet("{id}", Name = "GetStudentById")]
    [ValidateId]
    public async Task<ActionResult<StudentDto>> GetById(int id) => Ok(await _studentService.GetStudentByIdAsync(id));

    [HttpPost(Name = "CreateStudent")]
    [Authorize(Roles = "Admin")] // so the st can be create by two ways one thtough the create api by admin and one through registering byy st
    /// <summary>
    /// [FromBody] Creates a new student from the request body.
    /// </summary>

    public async Task<ActionResult<StudentDto>> Create([FromBody] CreateStudentDto dto)
    {
        var student = await _studentService.CreateStudentAsync(dto);
        return CreatedAtRoute("GetStudentById", new { id = student.StudentId }, student);
    }

    //
    //
    //

    [HttpPut("{id}", Name = "UpdateStudent")]
    [ValidateId]
    [Authorize(Policy = "OwnStudentOrAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentDto dto)
    {
        await _studentService.UpdateStudentAsync(id, dto);
        return NoContent();
    }

    //
    //
    //

    [HttpDelete("{id}", Name = "DeleteStudent")]
    [ValidateId]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _studentService.DeleteStudentAsync(id);
        return NoContent();
    }
}
