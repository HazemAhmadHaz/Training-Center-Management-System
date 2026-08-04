using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Repositories.Interfaces;
using TrainingCenterAPI.Services.Interfaces;
using TrainingCenterAPI.Services.Security;
using TrainingCenterAPI.Utilities.Exceptions;
using TrainingCenterAPI.Utilities.Helpers;

namespace TrainingCenterAPI.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IPasswordHasher _passwordHasher;

    public StudentService(
        IStudentRepository studentRepository,
        IPasswordHasher passwordHasher)
    {
        _studentRepository = studentRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<(IEnumerable<StudentDto> Items, int TotalCount)> GetAllStudentsAsync(
        StudentStatus? status,
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        return await _studentRepository
            .GetAllProjectedAsync(status, page, pageSize);
    }

    public async Task<StudentDto> GetStudentByIdAsync(int id)
    {
        var student = await _studentRepository
            .GetByIdProjectedAsync(id);

        return student
            ?? throw new NotFoundException(
                $"Student with ID {id} not found.");
    }

    public async Task<StudentDto> CreateStudentAsync(
        CreateStudentDto dto)
    {
        BusinessRuleHelper.ThrowIfExists(
            await _studentRepository.EmailExistsAsync(dto.Email),
            $"Student email '{dto.Email}' already exists.");

        var person = new Person
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            DateOfBirth = dto.DateOfBirth,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            Role = UserRole.Student
        };

        var student = new Student
        {
            Person = person,
            Status = StudentStatus.Active,
            RegisteredAt = DateTime.UtcNow
        };

        await _studentRepository.AddAsync(student);
        await _studentRepository.SaveChangesAsync();

        return (await _studentRepository
            .GetByIdProjectedAsync(student.StudentId))!;
    }

    public async Task UpdateStudentAsync(
        int id,
        UpdateStudentDto dto)
    {
        var student = await _studentRepository.GetByIdWithPersonAsync(id);

        if (student == null)
        {
            throw new NotFoundException(
                $"Student with ID {id} not found.");
        }

        BusinessRuleHelper.ThrowIfExists(
            await _studentRepository.EmailExistsAsync(dto.Email, id),
            $"Student email '{dto.Email}' already exists.");

        EnsureValidStatusTransition(
            student.Status,
            dto.Status);

        student.Person.FirstName = dto.FirstName;
        student.Person.LastName = dto.LastName;
        student.Person.Email = dto.Email;
        student.Person.DateOfBirth = dto.DateOfBirth;
        student.Person.PhoneNumber = dto.PhoneNumber;

        student.Status = dto.Status;

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
            {
                throw new BusinessRuleException(
                    "Current password is required to change password.");
            }

            if (!_passwordHasher.VerifyPassword(
                    dto.CurrentPassword,
                    student.Person.PasswordHash))
            {
                throw new UnauthorizedAccessException(
                    "Current password is incorrect.");
            }

            student.Person.PasswordHash =
                _passwordHasher.HashPassword(dto.NewPassword);
        }

        _studentRepository.Update(student);
        await _studentRepository.SaveChangesAsync();
    }

    public async Task DeleteStudentAsync(int id)
    {
        var student = await _studentRepository
            .GetByIdAsync(id);

        if (student == null)
        {
            throw new NotFoundException(
                $"Student with ID {id} not found.");
        }

        BusinessRuleHelper.ThrowIfExists(
            await _studentRepository.HasEnrollmentsAsync(id),
            "Cannot delete a student with enrollment history.");

        _studentRepository.Delete(student);
        await _studentRepository.SaveChangesAsync();
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        int? excludeStudentId = null)
    {
        return await _studentRepository
            .EmailExistsAsync(email, excludeStudentId);
    }

    public async Task<StudentDto?> GetStudentByEmailAsync(
        string email)
    {
        var student = await _studentRepository
            .GetByEmailAsync(email);

        return student == null
            ? null
            : await _studentRepository
                .GetByIdProjectedAsync(student.StudentId);
    }

    private static void EnsureValidStatusTransition(
        StudentStatus current,
        StudentStatus requested)
    {
        var isValid =
            current == requested ||
            current switch
            {
                StudentStatus.Active =>
                    requested is StudentStatus.Suspended
                        or StudentStatus.Graduated,

                StudentStatus.Suspended =>
                    requested == StudentStatus.Active,

                StudentStatus.Graduated => false,

                _ => false
            };

        BusinessRuleHelper.ThrowIfNotExists(
            isValid,
            $"Cannot change student status from {current} to {requested}.");
    }

}