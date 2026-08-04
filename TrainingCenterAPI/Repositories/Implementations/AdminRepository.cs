using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TrainingCenterAPI.Data;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Repositories.Interfaces;

namespace TrainingCenterAPI.Repositories.Implementations;

public class AdminRepository : Repository<Admin>, IAdminRepository
{
    private static readonly Expression<Func<Admin, AdminDto>> AdminSelector =
        admin => new AdminDto
        {
            AdminId = admin.AdminId,
            FirstName = admin.Person.FirstName,
            LastName = admin.Person.LastName,
            Email = admin.Person.Email,
            CreatedAt = admin.CreatedAt
        };


    public AdminRepository(TrainingCenterDbContext context)
        : base(context)
    {
    }


    public async Task<(IEnumerable<AdminDto> Items, int TotalCount)>
        GetAllProjectedAsync(int page, int pageSize)
    {
        var query = _context.Admins
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(a => a.AdminId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(AdminSelector)
            .ToListAsync();

        return (items, totalCount);
    }


    public Task<AdminDto?> GetByIdProjectedAsync(int id)
    {
        return _context.Admins
            .AsNoTracking()
            .Where(a => a.AdminId == id)
            .Select(AdminSelector)
            .FirstOrDefaultAsync();
    }


    public Task<Admin?> GetByIdWithPersonAsync(int id)
    {
        return _context.Admins
            .Include(a => a.Person)
            .FirstOrDefaultAsync(a => a.AdminId == id);
    }


    public Task<bool> EmailExistsAsync(
        string email,
        int? excludeAdminId = null)
    {
        return _context.Admins.AnyAsync(a =>
            a.Person.Email == email &&
            a.AdminId != excludeAdminId);
    }


    public Task<Admin?> GetByEmailAsync(string email)
    {
        return _context.Admins
            .Include(a => a.Person)
            .FirstOrDefaultAsync(a =>
                a.Person.Email == email);
    }
}