using FluentValidation;
using TrainingCenterAPI.DTOs;

namespace TrainingCenterAPI.Utilities.Validators;

public class CreateStudentProfileDtoValidator : AbstractValidator<CreateStudentProfileDto>
{
    public CreateStudentProfileDtoValidator()
    {
        AddProfileRules();
    }

    private void AddProfileRules()
    {
        RuleFor(profile => profile.Address).MaximumLength(200).When(profile => profile.Address != null);
        RuleFor(profile => profile.City).MaximumLength(100).When(profile => profile.City != null);
        RuleFor(profile => profile.Country).MaximumLength(100).When(profile => profile.Country != null);
        RuleFor(profile => profile.Bio).MaximumLength(500).When(profile => profile.Bio != null);
        RuleFor(profile => profile.LinkedInUrl)
            .MaximumLength(200)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("LinkedIn URL must be an absolute URL.")
            .When(profile => !string.IsNullOrWhiteSpace(profile.LinkedInUrl));
    }
}
