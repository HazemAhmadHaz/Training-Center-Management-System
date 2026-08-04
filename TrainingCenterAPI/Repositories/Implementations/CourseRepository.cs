using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TrainingCenterAPI.Data;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Repositories.Interfaces;

/// <summary>
/// Course-specific repository implementation. Inherits generic CRUD from
/// Repository&lt;Course&gt;. Uses a LINQ projection (CourseSelector) to shape
/// query results directly into CourseDto at the database level, avoiding
/// loading full Instructor entities just to get the instructor's name.
/// Also provides DB-backed checks (code uniqueness, instructor existence,
/// active enrollments) that the Service layer uses to enforce business rules.
/// </summary>

/// <summary>
/// Principle: Single Responsibility + Liskov Substitution.
/// Only responsible for Course's own data access logic. Because it
/// correctly implements ICourseRepository, any class satisfying that
/// interface could substitute it without breaking CourseService.
/// </summary>

namespace TrainingCenterAPI.Repositories.Implementations
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(TrainingCenterDbContext context) : base(context)
        {
        }

        private static readonly Expression<Func<Course, CourseDto>> CourseSelector =
            c => new CourseDto
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Code = c.Code,
                Description = c.Description,
                Price = c.Price,
                Level = c.Level,
                DurationHours = c.DurationHours,
                Status = c.Status,
                InstructorId = c.InstructorId,
                InstructorName = c.Instructor.Person.FirstName + " " + c.Instructor.Person.LastName,
                CreatedAt = c.CreatedAt,
                PublishedAt = c.PublishedAt
            };

        public async Task<(IEnumerable<CourseDto> Items, int TotalCount)> GetAllProjectedAsync(CourseLevel? level, CourseStatus? status, int page, int pageSize)
        {
            var query = _context.Courses.AsNoTracking()  //→ No SQL executed yet.
                .AsQueryable();                          //query is just an object representing "SELECT * FROM Courses" — not run.


            if (level.HasValue)
            {
                query = query.Where(c => c.Level == level.Value); //→ Still nothing executed.
                                                                  //Now the plan is "SELECT * FROM Courses WHERE Level = @level AND Status = @status".
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value); //→ Still nothing executed.
                                                                    //Now the plan is "SELECT * FROM Courses WHERE Level = @level AND Status = @status".
            }

            var totalCount = await query.CountAsync(); //→ THIS is when EF Core finally sends SQL to the database — but by now,
                                                       //the query already includes both WHERE clauses (only the ones that were actually added).
                                                       //SQL Server does the filtering, not C#. The database never sends back unfiltered rows first.

            var items = await query
                .OrderBy(c => c.CourseId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(CourseSelector)
                .ToListAsync();

            return (items, totalCount);
        }
        public async Task<CourseDto?> GetByIdProjectedAsync(int id)
        {
            return await _context.Courses.AsNoTracking().Where(c => c.CourseId == id).Select(CourseSelector).FirstOrDefaultAsync();
        }
        public async Task<bool> CodeExistsAsync(string code, int? excludeCourseId = null)
        {
            return await _context.Courses.AnyAsync(c => c.Code == code && c.CourseId != excludeCourseId);
        }
        public async Task<bool> InstructorExistsAsync(int instructorId)
        {
            return await _context.Instructors.AnyAsync(i => i.InstructorId == instructorId);
        }

        public async Task<bool> HasActiveEnrollmentsAsync(int courseId)
        {
            return await _context.Enrollments.AnyAsync(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active);
        }

        public async Task<IEnumerable<CourseDto>> GetByInstructorIdAsync(int instructorId)
        {
            return await _context.Courses
                .AsNoTracking()
                .Where(c => c.InstructorId == instructorId)
                .OrderBy(c => c.CourseId)
                .Select(CourseSelector)
                .ToListAsync();
        }
    }
}


