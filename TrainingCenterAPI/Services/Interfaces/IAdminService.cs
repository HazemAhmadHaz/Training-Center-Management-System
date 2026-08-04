using TrainingCenterAPI.DTOs;

namespace TrainingCenterAPI.Services.Interfaces;

public interface IAdminService
{
    Task<(IEnumerable<AdminDto> Items, int TotalCount)> GetAllAdminsAsync(
        int page,
        int pageSize);

    Task<AdminDto> GetAdminByIdAsync(int id);

    Task<AdminDto> CreateAdminAsync(CreateAdminDto dto);

    Task UpdateAdminAsync(int id, UpdateAdminDto dto);

    Task DeleteAdminAsync(int id);

    Task<bool> EmailExistsAsync(
        string email,
        int? excludeAdminId = null);

    Task<AdminDto?> GetAdminByEmailAsync(string email);
}