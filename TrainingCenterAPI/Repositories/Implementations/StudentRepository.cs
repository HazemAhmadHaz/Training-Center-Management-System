using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TrainingCenterAPI.Data;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Repositories.Interfaces;

namespace TrainingCenterAPI.Repositories.Implementations;

public class StudentRepository : Repository<Student>, IStudentRepository
{
    private static readonly Expression<Func<Student, StudentDto>> StudentSelector =
        student => new StudentDto
        {
            StudentId = student.StudentId,
            FirstName = student.Person.FirstName,
            LastName = student.Person.LastName,
            Email = student.Person.Email,
            DateOfBirth = student.Person.DateOfBirth,
            RegisteredAt = student.RegisteredAt,
            PhoneNumber = student.Person.PhoneNumber,
            Status = student.Status
        };

    public StudentRepository(TrainingCenterDbContext context)
        : base(context)
    {
    }

    public async Task<(IEnumerable<StudentDto> Items, int TotalCount)> GetAllProjectedAsync(
        StudentStatus? status,
        int page,
        int pageSize)
    {
        var query = _context.Students
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(student =>
                student.Status == status.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(student => student.StudentId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(StudentSelector)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<StudentDto?> GetByIdProjectedAsync(int id) =>
        _context.Students
            .AsNoTracking()
            .Where(student => student.StudentId == id)
            .Select(StudentSelector)
            .FirstOrDefaultAsync();

    public Task<bool> EmailExistsAsync(
        string email,
        int? excludeStudentId = null) =>
        _context.Students.AnyAsync(student =>
            student.Person.Email == email &&
            student.StudentId != excludeStudentId);

    public Task<bool> HasEnrollmentsAsync(int studentId) =>
        _context.Enrollments.AnyAsync(enrollment =>
            enrollment.StudentId == studentId);

    public Task<Student?> GetByEmailAsync(string email) =>
        _context.Students
            .Include(student => student.Person)
            .FirstOrDefaultAsync(student =>
                student.Person.Email == email);
    public async Task<Student?> GetByPersonIdAsync(int personId)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s => s.PersonId == personId);
    }
    public Task<Student?> GetByIdWithPersonAsync(int id)
    {
        return _context.Students
            .Include(s => s.Person)
            .FirstOrDefaultAsync(s => s.StudentId == id);
    }
}