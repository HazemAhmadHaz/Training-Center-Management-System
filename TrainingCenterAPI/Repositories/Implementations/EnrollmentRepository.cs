using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TrainingCenterAPI.Data;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Repositories.Interfaces;

namespace TrainingCenterAPI.Repositories.Implementations;

public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
{
    private static readonly Expression<Func<Enrollment, EnrollmentDto>> EnrollmentSelector =
        enrollment => new EnrollmentDto
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentId = enrollment.StudentId,
            StudentName = enrollment.Student.Person.FirstName + " " + enrollment.Student.Person.LastName,
            CourseTitle = enrollment.Course.Title,
            CourseCode = enrollment.Course.Code,
            EnrollmentDate = enrollment.EnrollmentDate,
            CompletionDate = enrollment.CompletionDate,
            ProgressPercent = enrollment.ProgressPercent,
            FinalGrade = enrollment.FinalGrade,
            Status = enrollment.Status
        };

    public EnrollmentRepository(TrainingCenterDbContext context) : base(context) { }

    public async Task<(IEnumerable<EnrollmentDto> Items, int TotalCount)> GetAllProjectedAsync(
        int? studentId, int? courseId, EnrollmentStatus? status, int page, int pageSize)
    {
        var query = _context.Enrollments.AsNoTracking().AsQueryable();

        if (studentId.HasValue)
        {
            query = query.Where(enrollment => enrollment.StudentId == studentId.Value);
        }

        if (courseId.HasValue)
        {
            query = query.Where(enrollment => enrollment.CourseId == courseId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(enrollment => enrollment.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(enrollment => enrollment.EnrollmentId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(EnrollmentSelector)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<EnrollmentDto?> GetByIdProjectedAsync(int id) =>
        _context.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.EnrollmentId == id)
            .Select(EnrollmentSelector)
            .FirstOrDefaultAsync();

    public Task<bool> StudentExistsAsync(int studentId) =>
        _context.Students.AnyAsync(student => student.StudentId == studentId);

    public Task<bool> IsStudentActiveAsync(int studentId) =>
        _context.Students.AnyAsync(student => student.StudentId == studentId && student.Status == StudentStatus.Active);

    public Task<bool> CourseExistsAsync(int courseId) =>
        _context.Courses.AnyAsync(course => course.CourseId == courseId);

    public Task<bool> IsCoursePublishedAsync(int courseId) =>
        _context.Courses.AnyAsync(course => course.CourseId == courseId && course.Status == CourseStatus.Published);

    public Task<bool> ExistsForStudentAndCourseAsync(int studentId, int courseId) =>
        _context.Enrollments.AnyAsync(enrollment =>
            enrollment.StudentId == studentId && enrollment.CourseId == courseId);
}
