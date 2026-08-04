using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TrainingCenterAPI.Data;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Repositories.Interfaces;

namespace TrainingCenterAPI.Repositories.Implementations;

public class InstructorRepository : Repository<Instructor>, IInstructorRepository
{
    private static readonly Expression<Func<Instructor, InstructorDto>> InstructorSelector =
        instructor => new InstructorDto
        {
            InstructorId = instructor.InstructorId,
            FirstName = instructor.Person.FirstName,
            LastName = instructor.Person.LastName,
            Email = instructor.Person.Email,
            HireDate = instructor.HireDate,
            Salary = instructor.Salary,
            ManagerId = instructor.ManagerId,
            ManagerName = instructor.Manager == null
                ? null
                : instructor.Manager.Person.FirstName + " " +
                  instructor.Manager.Person.LastName,
            IsActive = instructor.IsActive
        };

    public InstructorRepository(TrainingCenterDbContext context)
        : base(context)
    {
    }

    public async Task<(IEnumerable<InstructorDto> Items, int TotalCount)> GetAllProjectedAsync(
        bool? isActive,
        int page,
        int pageSize)
    {
        var query = _context.Instructors
            .AsNoTracking()
            .AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(instructor =>
                instructor.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(instructor => instructor.InstructorId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(InstructorSelector)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<InstructorDto?> GetByIdProjectedAsync(int id) =>
        _context.Instructors
            .AsNoTracking()
            .Where(instructor => instructor.InstructorId == id)
            .Select(InstructorSelector)
            .FirstOrDefaultAsync();

    public Task<bool> EmailExistsAsync(
        string email,
        int? excludeInstructorId = null) =>
        _context.Instructors.AnyAsync(instructor =>
            instructor.Person.Email == email &&
            instructor.InstructorId != excludeInstructorId);

    public Task<bool> HasCoursesAsync(int instructorId) =>
        _context.Courses.AnyAsync(course =>
            course.InstructorId == instructorId);

    public Task<bool> HasDirectReportsAsync(int instructorId) =>
        _context.Instructors.AnyAsync(instructor =>
            instructor.ManagerId == instructorId);

    public Task<Instructor?> GetByEmailAsync(string email) =>
        _context.Instructors
            .Include(instructor => instructor.Person)
            .FirstOrDefaultAsync(instructor =>
                instructor.Person.Email == email);
    public async Task<Instructor?> GetByPersonIdAsync(int personId)
    {
        return await _context.Instructors
            .FirstOrDefaultAsync(i => i.PersonId == personId);
    }
    public Task<Instructor?> GetByIdWithPersonAsync(int id)
    {
        return _context.Instructors
            .Include(i => i.Person)
            .FirstOrDefaultAsync(i => i.InstructorId == id);
    }
}