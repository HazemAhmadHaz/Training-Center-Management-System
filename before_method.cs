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

    public StudentService(IStudentRepository studentRepository, IPasswordHasher passwordHasher)
    {
        _studentRepository = studentRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<(IEnumerable<StudentDto> Items, int TotalCount)> GetAllStudentsAsync(StudentStatus? status, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;
        return await _studentRepository.GetAllProjectedAsync(status, page, pageSize);
    }

    public async Task<StudentDto> GetStudentByIdAsync(int id)
    {
        var student = await _studentRepository.GetByIdProjectedAsync(id);
        return student ?? throw new NotFoundException($"Student with ID {id} not found.");
    }

    public async Task<StudentDto> CreateStudentAsync(CreateStudentDto dto)
    {
        BusinessRuleHelper.ThrowIfExists(
            await _studentRepository.EmailExistsAsync(dto.Email),
            $"Student email '{dto.Email}' already exists.");

        var student = new Student
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            DateOfBirth = dto.DateOfBirth,
            PhoneNumber = dto.PhoneNumber,
            Status = StudentStatus.Active,
            RegisteredAt = DateTime.UtcNow,
            PasswordHash = "", // Will be updated later by AuthController
            Role = "Student" // Default role for students
        };

        await _studentRepository.AddAsync(student);
        await _studentRepository.SaveChangesAsync();
