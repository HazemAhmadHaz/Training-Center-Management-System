using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Repositories.Implementations;
using TrainingCenterAPI.Repositories.Interfaces;
using TrainingCenterAPI.Services.Interfaces;
using TrainingCenterAPI.Utilities.Exceptions;
using TrainingCenterAPI.Utilities.Helpers;

namespace TrainingCenterAPI.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;

    public EnrollmentService(IEnrollmentRepository enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<(IEnumerable<EnrollmentDto> Items, int TotalCount)> GetAllEnrollmentsAsync(
        int? studentId, int? courseId, EnrollmentStatus? status, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;
        return await _enrollmentRepository.GetAllProjectedAsync(studentId, courseId, status, page, pageSize);
    }

    public async Task<EnrollmentDto> GetEnrollmentByIdAsync(int id)
    {
        var enrollment = await _enrollmentRepository.GetByIdProjectedAsync(id);
        return enrollment ?? throw new NotFoundException($"Enrollment with ID {id} not found.");
    }

    public async Task<EnrollmentDto> CreateEnrollmentAsync(CreateEnrollmentDto dto)
    {
        await EnsureStudentCanEnrollAsync(dto.StudentId);
        await EnsureCourseCanAcceptEnrollmentsAsync(dto.CourseId);

        BusinessRuleHelper.ThrowIfExists(
            await _enrollmentRepository.ExistsForStudentAndCourseAsync(dto.StudentId, dto.CourseId),
            "This student is already enrolled in this course.");

        var enrollment = new Enrollment
        {
            StudentId = dto.StudentId,
            CourseId = dto.CourseId,
            EnrollmentDate = DateTime.UtcNow,
            ProgressPercent = 0,
            Status = EnrollmentStatus.Active
        };

        await _enrollmentRepository.AddAsync(enrollment);
        await _enrollmentRepository.SaveChangesAsync();
        return (await _enrollmentRepository.GetByIdProjectedAsync(enrollment.EnrollmentId))!;
    }

    public async Task UpdateEnrollmentAsync(int id, UpdateEnrollmentDto dto)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Enrollment with ID {id} not found.");

        EnsureValidStatusTransition(enrollment.Status, dto.Status);
        EnsureCompletionValuesAreValid(dto);

        // this is intended (correcting a grading mistake) and Completed enrollments is not fully locked

        enrollment.ProgressPercent = dto.ProgressPercent;
        enrollment.FinalGrade = dto.FinalGrade;
        enrollment.Status = dto.Status;

        if (dto.Status == EnrollmentStatus.Completed && enrollment.CompletionDate == null)
        {
            enrollment.CompletionDate = DateTime.UtcNow;
        }
        else if (dto.Status != EnrollmentStatus.Completed)
        {
            enrollment.CompletionDate = null;
            enrollment.FinalGrade = null;
        }

        _enrollmentRepository.Update(enrollment);
        await _enrollmentRepository.SaveChangesAsync();
    }

    public async Task DeleteEnrollmentAsync(int id)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Enrollment with ID {id} not found.");

        BusinessRuleHelper.ThrowIfNotExists(
            enrollment.Status == EnrollmentStatus.Dropped,
            "Only dropped enrollments can be deleted.");

        _enrollmentRepository.Delete(enrollment);
        await _enrollmentRepository.SaveChangesAsync();
    }

    private async Task EnsureStudentCanEnrollAsync(int studentId)
    {
        if (!await _enrollmentRepository.StudentExistsAsync(studentId))
        {
            throw new NotFoundException($"Student with ID {studentId} not found.");
        }

        BusinessRuleHelper.ThrowIfNotExists(
            await _enrollmentRepository.IsStudentActiveAsync(studentId),
            "Only active students can enroll in courses.");
    }

    private async Task EnsureCourseCanAcceptEnrollmentsAsync(int courseId)
    {
        if (!await _enrollmentRepository.CourseExistsAsync(courseId))
        {
            throw new NotFoundException($"Course with ID {courseId} not found.");
        }

        BusinessRuleHelper.ThrowIfNotExists(
            await _enrollmentRepository.IsCoursePublishedAsync(courseId),
            "Students can enroll only in published courses.");
    }

    private static void EnsureValidStatusTransition(EnrollmentStatus current, EnrollmentStatus requested)
    {
        var isValid = current == requested || current switch
        {
            EnrollmentStatus.Active => requested is EnrollmentStatus.Completed or EnrollmentStatus.Dropped,
            EnrollmentStatus.Dropped => requested == EnrollmentStatus.Active,
            EnrollmentStatus.Completed => false,
            _ => false
        };

        BusinessRuleHelper.ThrowIfNotExists(
            isValid,
            $"Cannot change enrollment status from {current} to {requested}.");
    }

    private static void EnsureCompletionValuesAreValid(UpdateEnrollmentDto dto)
    {
        if (dto.Status == EnrollmentStatus.Completed)
        {
            BusinessRuleHelper.ThrowIfNotExists(
                dto.ProgressPercent == 100,
                "A completed enrollment must have 100 percent progress.");
            BusinessRuleHelper.ThrowIfNotExists(
                dto.FinalGrade.HasValue,
                "A completed enrollment must have a final grade.");
        }
        else
        {
            BusinessRuleHelper.ThrowIfExists(
                dto.FinalGrade.HasValue,
                "Only completed enrollments can have a final grade.");
        }
    }

    public async Task<Enrollment?> GetByIdAsync(int id)
    {
        return await _enrollmentRepository
            .GetByIdAsync(id);
    }
}
