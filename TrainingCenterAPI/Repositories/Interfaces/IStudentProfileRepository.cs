using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Models;

namespace TrainingCenterAPI.Repositories.Interfaces;

public interface IStudentProfileRepository : IRepository<StudentProfile>
{
    Task<StudentProfileDto?> GetProjectedByStudentIdAsync(int studentId);
    Task<bool> ExistsAsync(int studentId);
}
