using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Models;

/// <summary>
/// Course-specific repository contract. Inherits basic CRUD from
/// IRepository&lt;Course&gt;, and adds Course-only operations: DTO-projected
/// queries (GetAllProjectedAsync, GetByIdProjectedAsync) that include the
/// instructor's name, plus DTO-based Add/Update, and existence checks
/// (CodeExistsAsync, InstructorExistsAsync, HasActiveEnrollmentsAsync)
/// used for business rule validation in the Service layer.
/// </summary>

/// <summary>
/// Principle: Interface Segregation + Dependency Inversion.
/// Exposes only Course-specific methods (not shared by Student/Instructor),
/// so CourseService depends on a focused abstraction instead of one
/// giant interface shared across unrelated entities.
/// </summary>

namespace TrainingCenterAPI.Repositories.Interfaces
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<(IEnumerable<CourseDto> Items, int TotalCount)> GetAllProjectedAsync(CourseLevel? level, CourseStatus? status, int page, int pageSize);
        Task<CourseDto?> GetByIdProjectedAsync(int id);
        Task<bool> CodeExistsAsync(string code, int? excludeCourseId = null);
        Task<bool> InstructorExistsAsync(int instructorId);
        Task<bool> HasActiveEnrollmentsAsync(int courseId);
        Task<IEnumerable<CourseDto>> GetByInstructorIdAsync(int instructorId);
    }
}