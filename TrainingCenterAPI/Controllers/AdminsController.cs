using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Services.Interfaces;
using TrainingCenterAPI.Utilities.Filters;

namespace TrainingCenterAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminsController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    //
    //
    //

    [HttpGet(Name = "GetAllAdmins")]
    
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var (admins, totalCount) =
            await _adminService.GetAllAdminsAsync(page, pageSize);

        return Ok(new
        {
            totalCount,
            page,
            pageSize,
            items = admins
        });
    }

    //
    //
    //

    [HttpGet("{id}", Name = "GetAdminById")]
    [ValidateId]
    
    public async Task<ActionResult<AdminDto>> GetById(int id)
    {
        return Ok(await _adminService.GetAdminByIdAsync(id));
    }

    //
    //
    //

    [HttpPost(Name = "CreateAdmin")]
    
    public async Task<ActionResult<AdminDto>> Create(
        [FromBody] CreateAdminDto dto)
    {
        var admin = await _adminService.CreateAdminAsync(dto);

        return CreatedAtRoute(
            "GetAdminById",
            new { id = admin.AdminId },
            admin);
    }

    //
    //
    //

    [HttpPut("{id}", Name = "UpdateAdmin")]
    [ValidateId]
    
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateAdminDto dto)
    {
        await _adminService.UpdateAdminAsync(id, dto);

        return NoContent();
    }

    //
    //
    //

    [HttpDelete("{id}", Name = "DeleteAdmin")]
    [ValidateId]
    
    public async Task<IActionResult> Delete(int id)
    {
        await _adminService.DeleteAdminAsync(id);

        return NoContent();
    }
}