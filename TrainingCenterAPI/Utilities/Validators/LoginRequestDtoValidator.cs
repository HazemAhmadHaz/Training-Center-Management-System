using FluentValidation;
using TrainingCenterAPI.DTOs.Auth;

namespace TrainingCenterAPI.Validators.Auth;

public class LoginRequestDtoValidator
    : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150);


        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);
    }
}