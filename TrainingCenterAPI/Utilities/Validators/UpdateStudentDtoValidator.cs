using FluentValidation;
using TrainingCenterAPI.DTOs;

namespace TrainingCenterAPI.Utilities.Validators;

public class UpdateStudentDtoValidator : PersonDtoValidator<UpdateStudentDto>
{
    public UpdateStudentDtoValidator()
    {
        RuleFor(student => student.NewPassword)
            .MinimumLength(6)
            .When(student => !string.IsNullOrWhiteSpace(student.NewPassword));
    }
}