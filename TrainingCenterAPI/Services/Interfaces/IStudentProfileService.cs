using TrainingCenterAPI.DTOs;

namespace TrainingCenterAPI.Services.Interfaces;

public interface IStudentProfileService
{
    Task<StudentProfileDto> GetProfileAsync(int studentId);
    Task<StudentProfileDto> CreateProfileAsync(int studentId, CreateStudentProfileDto dto);
    Task UpdateProfileAsync(int studentId, UpdateStudentProfileDto dto);
    Task DeleteProfileAsync(int studentId);
}
