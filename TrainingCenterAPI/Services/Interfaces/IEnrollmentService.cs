using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Models;

namespace TrainingCenterAPI.Services.Interfaces;

public interface IEnrollmentService
{
    Task<(IEnumerable<EnrollmentDto> Items, int TotalCount)> GetAllEnrollmentsAsync(
        int? studentId, int? courseId, EnrollmentStatus? status, int page, int pageSize);
    Task<EnrollmentDto> GetEnrollmentByIdAsync(int id);
    Task<EnrollmentDto> CreateEnrollmentAsync(CreateEnrollmentDto dto);
    Task UpdateEnrollmentAsync(int id, UpdateEnrollmentDto dto);
    Task DeleteEnrollmentAsync(int id);

    Task<Enrollment?> GetByIdAsync(int id);
}
