using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Models;

namespace TrainingCenterAPI.Repositories.Interfaces;

public interface IEnrollmentRepository : IRepository<Enrollment>
{
    Task<(IEnumerable<EnrollmentDto> Items, int TotalCount)> GetAllProjectedAsync(
        int? studentId, int? courseId, EnrollmentStatus? status, int page, int pageSize);
    Task<EnrollmentDto?> GetByIdProjectedAsync(int id);
    Task<bool> StudentExistsAsync(int studentId);
    Task<bool> IsStudentActiveAsync(int studentId);
    Task<bool> CourseExistsAsync(int courseId);
    Task<bool> IsCoursePublishedAsync(int courseId);
    Task<bool> ExistsForStudentAndCourseAsync(int studentId, int courseId);
}
