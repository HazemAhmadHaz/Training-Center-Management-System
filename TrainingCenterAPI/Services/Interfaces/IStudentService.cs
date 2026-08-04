using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Enums;

namespace TrainingCenterAPI.Services.Interfaces;

public interface IStudentService
{
    Task<(IEnumerable<StudentDto> Items, int TotalCount)> GetAllStudentsAsync(StudentStatus? status, int page, int pageSize);
    Task<StudentDto> GetStudentByIdAsync(int id);
    Task<StudentDto> CreateStudentAsync(CreateStudentDto dto);
    Task UpdateStudentAsync(int id, UpdateStudentDto dto);
    Task DeleteStudentAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeStudentId = null);
    Task<StudentDto?> GetStudentByEmailAsync(string email);
}
