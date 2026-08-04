using TrainingCenterAPI.Models;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Enums;

/// <summary>
/// Course-specific business logic contract. Defines the operations
/// CoursesController is allowed to call — CreateCourse, UpdateCourse,
/// DeleteCourse, etc. — without exposing how they're implemented
/// or which repository methods they call.
/// Principle: Dependency Inversion + Interface Segregation.
/// CoursesController depends on this abstraction, not on CourseService
/// directly. Only Course-relevant methods are exposed, not a shared
/// interface bloated with Student/Instructor operations.
/// </summary>

/// <summary>
/// Business logic is inherently entity-specific 
/// — Course validates code uniqueness and instructor existence;
/// Student would validate different things entirely 
/// (maybe email uniqueness, enrollment limits).
/// Unlike CRUD, there's no meaningful "generic business rule" to extract 
/// — every entity's rules are different by nature.
/// Forcing a shared Service<T> base would either sit empty
/// (no real shared logic) or force irrelevant methods onto entities that don't need them,
/// violating Interface Segregation.
/// So each entity gets its own ICourseService/CourseService,
/// IStudentService/StudentService, etc. — no generic base needed,
/// because Services don't have a "GetAll/Add/Delete" pattern the way data access does.
/// </summary>

namespace TrainingCenterAPI.Services.Interfaces
{
    public interface ICourseService
    {
        Task<(IEnumerable<CourseDto> Items, int TotalCount)> GetAllCoursesAsync(CourseLevel? level, CourseStatus? status, int page, int pageSize);
        Task<CourseDto> GetCourseByIdAsync(int id);
        Task<CourseDto> CreateCourseAsync(CreateCourseDto dto);
        Task UpdateCourseAsync(int id, UpdateCourseDto dto);
        Task DeleteCourseAsync(int id);
        Task PublishCourseAsync(int id);
        Task<IEnumerable<CourseDto>> GetCoursesByInstructorAsync(int instructorId);
    }
}