using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Models;

namespace TrainingCenterAPI.Repositories.Interfaces;

public interface IStudentRepository : IRepository<Student>
{
    Task<(IEnumerable<StudentDto> Items, int TotalCount)> GetAllProjectedAsync(StudentStatus? status, int page, int pageSize);
    Task<StudentDto?> GetByIdProjectedAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeStudentId = null);
    Task<bool> HasEnrollmentsAsync(int studentId);
    Task<Student?> GetByEmailAsync(string email);   // Student version
    Task<Student?> GetByPersonIdAsync(int personId);
    Task<Student?> GetByIdWithPersonAsync(int id);

}
