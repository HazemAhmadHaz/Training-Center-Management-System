using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Models;

namespace TrainingCenterAPI.Services.Interfaces;

public interface IInstructorService
{
    Task<(IEnumerable<InstructorDto> Items, int TotalCount)> GetAllInstructorsAsync(bool? isActive, int page, int pageSize);
    Task<InstructorDto> GetInstructorByIdAsync(int id);
    Task<InstructorDto> CreateInstructorAsync(CreateInstructorDto dto);
    Task UpdateInstructorAsync(int id, UpdateInstructorDto dto);
    Task DeleteInstructorAsync(int id);
    Task<Instructor?> GetByPersonIdAsync(int personId);
}
