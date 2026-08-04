using FluentValidation;
using TrainingCenterAPI.DTOs;

namespace TrainingCenterAPI.Utilities.Validators;

public class CreateStudentDtoValidator : PersonDtoValidator<CreateStudentDto>
{
    public CreateStudentDtoValidator()
    {
        RuleFor(student => student.Password)
            .NotEmpty()
            .MinimumLength(6);
    }
}