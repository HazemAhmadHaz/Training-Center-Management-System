using FluentValidation;
using TrainingCenterAPI.DTOs;

namespace TrainingCenterAPI.Utilities.Validators;

public class CreateAdminDtoValidator : PersonDtoValidator<CreateAdminDto>
{
    public CreateAdminDtoValidator()
    {
        RuleFor(admin => admin.Password)
            .NotEmpty()
            .MinimumLength(6)
            .EmailAddress();
    }
}