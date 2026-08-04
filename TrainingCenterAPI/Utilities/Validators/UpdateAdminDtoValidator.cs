using FluentValidation;
using TrainingCenterAPI.DTOs;

namespace TrainingCenterAPI.Utilities.Validators;

public class UpdateAdminDtoValidator : PersonDtoValidator<UpdateAdminDto>
{
    public UpdateAdminDtoValidator()
    {
        RuleFor(admin => admin.NewPassword)
            .MinimumLength(6)
            .When(admin => !string.IsNullOrWhiteSpace(admin.NewPassword));
    }
}