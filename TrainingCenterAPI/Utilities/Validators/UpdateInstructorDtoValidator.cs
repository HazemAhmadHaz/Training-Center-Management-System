using FluentValidation;
using TrainingCenterAPI.DTOs;

namespace TrainingCenterAPI.Utilities.Validators;

public class UpdateInstructorDtoValidator : PersonDtoValidator<UpdateInstructorDto>
{
    public UpdateInstructorDtoValidator()
    {
        RuleFor(instructor => instructor.HireDate)
            .Must(hireDate =>
                hireDate <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Hire date cannot be in the future.");

        RuleFor(instructor => instructor.Salary)
            .GreaterThanOrEqualTo(0);

        RuleFor(instructor => instructor.ManagerId)
            .GreaterThan(0)
            .When(instructor => instructor.ManagerId.HasValue);
    }
}