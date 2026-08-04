using FluentValidation;
using TrainingCenterAPI.DTOs;

namespace TrainingCenterAPI.Utilities.Validators;

public class CreateEnrollmentDtoValidator : AbstractValidator<CreateEnrollmentDto>
{
    public CreateEnrollmentDtoValidator()
    {
        RuleFor(enrollment => enrollment.StudentId).GreaterThan(0);
        RuleFor(enrollment => enrollment.CourseId).GreaterThan(0);
    }
}
