using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TrainingCenterAPI.Data;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Repositories.Interfaces;

namespace TrainingCenterAPI.Repositories.Implementations;

public class StudentProfileRepository : Repository<StudentProfile>, IStudentProfileRepository
{
    private static readonly Expression<Func<StudentProfile, StudentProfileDto>> ProfileSelector =
        profile => new StudentProfileDto
        {
            StudentId = profile.StudentId,
            Address = profile.Address,
            City = profile.City,
            Country = profile.Country,
            Bio = profile.Bio,
            LinkedInUrl = profile.LinkedInUrl
        };

    public StudentProfileRepository(TrainingCenterDbContext context) : base(context) { }

    public Task<StudentProfileDto?> GetProjectedByStudentIdAsync(int studentId) =>
        _context.StudentProfiles.AsNoTracking()
            .Where(profile => profile.StudentId == studentId)
            .Select(ProfileSelector)
            .FirstOrDefaultAsync();

    public Task<bool> ExistsAsync(int studentId) =>
        _context.StudentProfiles.AnyAsync(profile => profile.StudentId == studentId);
}
