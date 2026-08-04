using FluentValidation;
using TrainingCenterAPI.DTOs.Auth;

namespace TrainingCenterAPI.Validators
{
    public class RegisterInstructorRequestDtoValidator : AbstractValidator<RegisterInstructorRequestDto>
    {
        public RegisterInstructorRequestDtoValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        }
    }
}