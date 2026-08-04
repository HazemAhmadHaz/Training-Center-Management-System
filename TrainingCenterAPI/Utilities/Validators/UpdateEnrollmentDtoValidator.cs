using FluentValidation;
using TrainingCenterAPI.DTOs;

namespace TrainingCenterAPI.Utilities.Validators;

public class UpdateEnrollmentDtoValidator : AbstractValidator<UpdateEnrollmentDto>
{
    public UpdateEnrollmentDtoValidator()
    {
        RuleFor(enrollment => enrollment.ProgressPercent).InclusiveBetween(0, 100);
        RuleFor(enrollment => enrollment.FinalGrade)
            .InclusiveBetween(0, 100)
            .When(enrollment => enrollment.FinalGrade.HasValue);
    }
}
