using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Models;

namespace TrainingCenterAPI.Repositories.Interfaces;

public interface IAdminRepository : IRepository<Admin>
{
    Task<(IEnumerable<AdminDto> Items, int TotalCount)> GetAllProjectedAsync(
        int page,
        int pageSize);

    Task<AdminDto?> GetByIdProjectedAsync(int id);

    Task<Admin?> GetByIdWithPersonAsync(int id);

    Task<bool> EmailExistsAsync(
        string email,
        int? excludeAdminId = null);

    Task<Admin?> GetByEmailAsync(string email);
}