using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Models;

namespace TrainingCenterAPI.Repositories.Interfaces;

public interface IInstructorRepository : IRepository<Instructor>
{
    Task<(IEnumerable<InstructorDto> Items, int TotalCount)> GetAllProjectedAsync(bool? isActive, int page, int pageSize);
    Task<InstructorDto?> GetByIdProjectedAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeInstructorId = null);
    Task<bool> HasCoursesAsync(int instructorId);
    Task<bool> HasDirectReportsAsync(int instructorId);
    Task<Instructor?> GetByEmailAsync(string email); // Instructor version
    Task<Instructor?> GetByPersonIdAsync(int personId);
    Task<Instructor?> GetByIdWithPersonAsync(int id);
}
