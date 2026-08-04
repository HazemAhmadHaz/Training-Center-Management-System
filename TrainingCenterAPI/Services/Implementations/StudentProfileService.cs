using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Repositories.Interfaces;
using TrainingCenterAPI.Services.Interfaces;
using TrainingCenterAPI.Utilities.Exceptions;
using TrainingCenterAPI.Utilities.Helpers;

namespace TrainingCenterAPI.Services.Implementations;

public class StudentProfileService : IStudentProfileService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentProfileRepository _profileRepository;

    public StudentProfileService(IStudentRepository studentRepository, IStudentProfileRepository profileRepository)
    {
        _studentRepository = studentRepository;
        _profileRepository = profileRepository;
    }

    public async Task<StudentProfileDto> GetProfileAsync(int studentId)
    {
        var profile = await _profileRepository.GetProjectedByStudentIdAsync(studentId);
        return profile ?? throw new NotFoundException($"Student profile for student ID {studentId} not found.");
    }

    public async Task<StudentProfileDto> CreateProfileAsync(int studentId, CreateStudentProfileDto dto)
    {
        await EnsureStudentExistsAsync(studentId);
        BusinessRuleHelper.ThrowIfExists(
            await _profileRepository.ExistsAsync(studentId),
            $"Student ID {studentId} already has a profile.");

        var profile = new StudentProfile
        {
            StudentId = studentId,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,
            Bio = dto.Bio,
            LinkedInUrl = dto.LinkedInUrl
        };

        await _profileRepository.AddAsync(profile);
        await _profileRepository.SaveChangesAsync();
        return (await _profileRepository.GetProjectedByStudentIdAsync(studentId))!;
    }

    public async Task UpdateProfileAsync(int studentId, UpdateStudentProfileDto dto)
    {
        var profile = await _profileRepository.GetByIdAsync(studentId)
            ?? throw new NotFoundException($"Student profile for student ID {studentId} not found.");

        profile.Address = dto.Address;
        profile.City = dto.City;
        profile.Country = dto.Country;
        profile.Bio = dto.Bio;
        profile.LinkedInUrl = dto.LinkedInUrl;

        _profileRepository.Update(profile);
        await _profileRepository.SaveChangesAsync();
    }

    public async Task DeleteProfileAsync(int studentId)
    {
        var profile = await _profileRepository.GetByIdAsync(studentId)
            ?? throw new NotFoundException($"Student profile for student ID {studentId} not found.");

        _profileRepository.Delete(profile);
        await _profileRepository.SaveChangesAsync();
    }

    private async Task EnsureStudentExistsAsync(int studentId)
    {
        if (await _studentRepository.GetByIdAsync(studentId) == null)
        {
            throw new NotFoundException($"Student with ID {studentId} not found.");
        }
    }
}
