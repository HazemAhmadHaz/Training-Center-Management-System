using FluentValidation;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.DTOs.Auth;

namespace TrainingCenterAPI.Validators.Auth;

public class RefreshTokenRequestDtoValidator
    : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}