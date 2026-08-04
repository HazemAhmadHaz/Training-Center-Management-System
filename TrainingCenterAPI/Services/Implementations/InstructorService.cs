using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Repositories.Interfaces;
using TrainingCenterAPI.Services.Interfaces;
using TrainingCenterAPI.Utilities.Exceptions;
using TrainingCenterAPI.Utilities.Helpers;
using TrainingCenterAPI.Services.Security;

namespace TrainingCenterAPI.Services.Implementations;

public class InstructorService : IInstructorService
{
    private readonly IInstructorRepository _instructorRepository;
    private readonly IPasswordHasher _passwordHasher;

    public InstructorService(
        IInstructorRepository instructorRepository,
        IPasswordHasher passwordHasher)
    {
        _instructorRepository = instructorRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<(IEnumerable<InstructorDto> Items, int TotalCount)> GetAllInstructorsAsync(
        bool? isActive,
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        return await _instructorRepository
            .GetAllProjectedAsync(isActive, page, pageSize);
    }

    public async Task<InstructorDto> GetInstructorByIdAsync(int id)
    {
        var instructor = await _instructorRepository
            .GetByIdProjectedAsync(id);

        return instructor
            ?? throw new NotFoundException(
                $"Instructor with ID {id} not found.");
    }

    public async Task<InstructorDto> CreateInstructorAsync(
        CreateInstructorDto dto)
    {
        BusinessRuleHelper.ThrowIfExists(
            await _instructorRepository.EmailExistsAsync(dto.Email),
            $"Instructor email '{dto.Email}' already exists.");

        if (dto.ManagerId.HasValue)
        {
            await EnsureActiveManagerAsync(dto.ManagerId.Value);
        }

        var person = new Person
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            DateOfBirth = dto.DateOfBirth,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            Role = Enums.UserRole.Instructor
        };

        var instructor = new Instructor
        {
            Person = person,
            HireDate = dto.HireDate,
            Salary = dto.Salary,
            ManagerId = dto.ManagerId,
            IsActive = true
        };

        await _instructorRepository.AddAsync(instructor);
        await _instructorRepository.SaveChangesAsync();

        return (await _instructorRepository
            .GetByIdProjectedAsync(instructor.InstructorId))!;
    }

    public async Task UpdateInstructorAsync(
        int id,
        UpdateInstructorDto dto)
    {
        var instructor = await _instructorRepository
            .GetByIdWithPersonAsync(id);

        if (instructor == null)
        {
            throw new NotFoundException(
                $"Instructor with ID {id} not found.");
        }

        BusinessRuleHelper.ThrowIfExists(
            await _instructorRepository.EmailExistsAsync(dto.Email, id),
            $"Instructor email '{dto.Email}' already exists.");

        if (dto.ManagerId.HasValue)
        {
            BusinessRuleHelper.ThrowIfExists(
                dto.ManagerId.Value == id,
                "An instructor cannot be their own manager.");

            await EnsureActiveManagerAsync(dto.ManagerId.Value);
        }

        instructor.Person.FirstName = dto.FirstName;
        instructor.Person.LastName = dto.LastName;
        instructor.Person.Email = dto.Email;
        instructor.Person.DateOfBirth = dto.DateOfBirth;
        instructor.Person.PhoneNumber = dto.PhoneNumber;

        instructor.HireDate = dto.HireDate;
        instructor.Salary = dto.Salary;
        instructor.ManagerId = dto.ManagerId;
        instructor.IsActive = dto.IsActive;

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
            {
                throw new BusinessRuleException(
                    "Current password is required to change password.");
            }

            if (!_passwordHasher.VerifyPassword(
                    dto.CurrentPassword,
                    instructor.Person.PasswordHash))
            {
                throw new UnauthorizedAccessException(
                    "Current password is incorrect.");
            }

            instructor.Person.PasswordHash =
                _passwordHasher.HashPassword(dto.NewPassword);
        }

        _instructorRepository.Update(instructor);
        await _instructorRepository.SaveChangesAsync();
    }

    public async Task DeleteInstructorAsync(int id)
    {
        var instructor = await _instructorRepository
            .GetByIdAsync(id);

        if (instructor == null)
        {
            throw new NotFoundException(
                $"Instructor with ID {id} not found.");
        }

        BusinessRuleHelper.ThrowIfExists(
            await _instructorRepository.HasCoursesAsync(id),
            "Cannot delete an instructor assigned to courses.");

        BusinessRuleHelper.ThrowIfExists(
            await _instructorRepository.HasDirectReportsAsync(id),
            "Cannot delete an instructor who manages other instructors.");

        _instructorRepository.Delete(instructor);
        await _instructorRepository.SaveChangesAsync();
    }

    private async Task EnsureActiveManagerAsync(int managerId)
    {
        var manager = await _instructorRepository
            .GetByIdAsync(managerId);

        if (manager == null)
        {
            throw new NotFoundException(
                $"Manager with ID {managerId} not found.");
        }

        BusinessRuleHelper.ThrowIfNotExists(
            manager.IsActive,
            "An inactive instructor cannot be assigned as a manager.");
    }

    public async Task<Instructor?> GetByPersonIdAsync(int personId)
    {
        return await _instructorRepository
            .GetByPersonIdAsync(personId);
    }
}