using TrainingCenterAPI.Repositories.Interfaces;
using TrainingCenterAPI.Services.Interfaces;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Utilities.Helpers;
using TrainingCenterAPI.Utilities.Exceptions;

namespace TrainingCenterAPI.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        public CourseService(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<(IEnumerable<CourseDto> Items, int TotalCount)> GetAllCoursesAsync(CourseLevel? level, CourseStatus? status, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            return await _courseRepository.GetAllProjectedAsync(level, status, page, pageSize);
        }


        public async Task<CourseDto> GetCourseByIdAsync(int id) // Note the return type changed from Task<CourseDto?> to Task<CourseDto> — no more ?,
                                                                // since this method now either returns a valid CourseDto or throws, never null.
            {
            var course = await _courseRepository.GetByIdProjectedAsync(id);
            if (course == null)
            {
                throw new NotFoundException($"Course with ID {id} not found.");
            }
            return course;
        }

        /// <summary>
        /// CreateCourseAsync never returns false — but it was never designed to. Look at its signature: Task<CourseDto> CreateCourseAsync(...) 
        /// — it always either returns an actual CourseDto (success) or throws (failure). There's no bool here to begin with, and there never was 
        /// — this method's return type was always "the created course," not "did it succeed."
        /// So how do YOU actually find out it failed? Through the HTTP response, not through checking a return value in code:
        /// BusinessRuleException is thrown
        /// ExceptionHandlingMiddleware catches it
        /// Client receives: 400 Bad Request, body: { "error": "Course code 'X' already exists." }
        /// </summary>
   
        public async Task<CourseDto> CreateCourseAsync(CreateCourseDto dto)
        {
            var codeExists = await _courseRepository.CodeExistsAsync(dto.Code);
            BusinessRuleHelper.ThrowIfExists(codeExists, $"Course code '{dto.Code}' already exists.");

            var instructorExists = await _courseRepository.InstructorExistsAsync(dto.InstructorId);
            BusinessRuleHelper.ThrowIfNotExists(instructorExists, $"Instructor with ID {dto.InstructorId} does not exist.");

            var course = new Course
            {
                Title = dto.Title,
                Code = dto.Code,
                Description = dto.Description,
                Price = dto.Price,
                Level = dto.Level,
                DurationHours = dto.DurationHours,
                InstructorId = dto.InstructorId,
                Status = CourseStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            await _courseRepository.AddAsync(course);
            await _courseRepository.SaveChangesAsync();

            return (await _courseRepository.GetByIdProjectedAsync(course.CourseId))!;
        }

        public async Task UpdateCourseAsync(int id, UpdateCourseDto dto)
        {
            var existing = await _courseRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new NotFoundException($"Course with ID {id} not found.");
            }

            var codeExists = await _courseRepository.CodeExistsAsync(dto.Code, id);
            BusinessRuleHelper.ThrowIfExists(codeExists, $"Course code '{dto.Code}' already exists.");

            var instructorExists = await _courseRepository.InstructorExistsAsync(dto.InstructorId);
            BusinessRuleHelper.ThrowIfNotExists(instructorExists, $"Instructor with ID {dto.InstructorId} does not exist.");

            existing.Title = dto.Title;
            existing.Code = dto.Code;
            existing.Description = dto.Description;
            existing.Price = dto.Price;
            existing.Level = dto.Level;
            existing.DurationHours = dto.DurationHours;
            existing.InstructorId = dto.InstructorId;

            if (dto.Status == CourseStatus.Published && existing.Status != CourseStatus.Published)
            {
                existing.PublishedAt = DateTime.UtcNow;
            }
            existing.Status = dto.Status;

            _courseRepository.Update(existing);
            await _courseRepository.SaveChangesAsync();
        }

        public async Task DeleteCourseAsync(int id)
        {
            var existing = await _courseRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new NotFoundException($"Course with ID {id} not found.");
            }

            var hasEnrollments = await _courseRepository.HasActiveEnrollmentsAsync(id);
            BusinessRuleHelper.ThrowIfExists(hasEnrollments, "Cannot delete a course with active enrollments.");

            _courseRepository.Delete(existing);
            await _courseRepository.SaveChangesAsync();
        }

        public async Task PublishCourseAsync(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                throw new NotFoundException($"Course with ID {id} not found.");
            }

            course.Status = CourseStatus.Published;
            course.PublishedAt = DateTime.UtcNow;

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesByInstructorAsync(int instructorId)
        {
            var instructorExists = await _courseRepository.InstructorExistsAsync(instructorId);
            BusinessRuleHelper.ThrowIfNotExists(instructorExists, $"Instructor with ID {instructorId} does not exist.");

            return await _courseRepository.GetByInstructorIdAsync(instructorId);
        }
    }
}
