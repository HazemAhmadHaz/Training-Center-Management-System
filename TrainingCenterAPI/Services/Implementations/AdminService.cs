using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Repositories.Interfaces;
using TrainingCenterAPI.Services.Interfaces;
using TrainingCenterAPI.Services.Security;
using TrainingCenterAPI.Utilities.Exceptions;
using TrainingCenterAPI.Utilities.Helpers;

namespace TrainingCenterAPI.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AdminService(
        IAdminRepository adminRepository,
        IPasswordHasher passwordHasher)
    {
        _adminRepository = adminRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<(IEnumerable<AdminDto> Items, int TotalCount)> GetAllAdminsAsync(
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        return await _adminRepository
            .GetAllProjectedAsync(page, pageSize);
    }

    public async Task<AdminDto> GetAdminByIdAsync(int id)
    {
        var admin = await _adminRepository
            .GetByIdProjectedAsync(id);

        return admin
            ?? throw new NotFoundException(
                $"Admin with ID {id} not found.");
    }

    public async Task<AdminDto> CreateAdminAsync(
        CreateAdminDto dto)
    {
        BusinessRuleHelper.ThrowIfExists(
            await _adminRepository.EmailExistsAsync(dto.Email),
            $"Admin email '{dto.Email}' already exists.");

        var person = new Person
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            DateOfBirth = dto.DateOfBirth,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            Role = UserRole.Admin
        };

        var admin = new Admin
        {
            Person = person,
            CreatedAt = DateTime.UtcNow
        };

        await _adminRepository.AddAsync(admin);
        await _adminRepository.SaveChangesAsync();

        return (await _adminRepository
            .GetByIdProjectedAsync(admin.AdminId))!;
    }

    public async Task UpdateAdminAsync(
        int id,
        UpdateAdminDto dto)
    {
        var admin = await _adminRepository
      .GetByIdWithPersonAsync(id);

        if (admin == null)
        {
            throw new NotFoundException(
                $"Admin with ID {id} not found.");
        }

        BusinessRuleHelper.ThrowIfExists(
            await _adminRepository.EmailExistsAsync(dto.Email, id),
            $"Admin email '{dto.Email}' already exists.");

        admin.Person.FirstName = dto.FirstName;
        admin.Person.LastName = dto.LastName;
        admin.Person.Email = dto.Email;
        admin.Person.DateOfBirth = dto.DateOfBirth;
        admin.Person.PhoneNumber = dto.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
            {
                throw new BusinessRuleException(
                    "Current password is required to change password.");
            }

            if (!_passwordHasher.VerifyPassword(
                    dto.CurrentPassword,
                    admin.Person.PasswordHash))
            {
                throw new UnauthorizedAccessException(
                    "Current password is incorrect.");
            }

            admin.Person.PasswordHash =
                _passwordHasher.HashPassword(dto.NewPassword);
        }

        _adminRepository.Update(admin);
        await _adminRepository.SaveChangesAsync();
    }

    public async Task DeleteAdminAsync(int id)
    {
        var admin = await _adminRepository
            .GetByIdAsync(id);

        if (admin == null)
        {
            throw new NotFoundException(
                $"Admin with ID {id} not found.");
        }

        _adminRepository.Delete(admin);
        await _adminRepository.SaveChangesAsync();
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        int? excludeAdminId = null)
    {
        return await _adminRepository
            .EmailExistsAsync(email, excludeAdminId);
    }

    public async Task<AdminDto?> GetAdminByEmailAsync(
        string email)
    {
        var admin = await _adminRepository
            .GetByEmailAsync(email);

        return admin == null
            ? null
            : await _adminRepository
                .GetByIdProjectedAsync(admin.AdminId);
    }
}