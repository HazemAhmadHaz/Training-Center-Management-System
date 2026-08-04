using FluentValidation;
using TrainingCenterAPI.DTOs;

namespace TrainingCenterAPI.Utilities.Validators;

public abstract class PersonDtoValidator<T> : AbstractValidator<T>
    where T : PersonDto
{
    protected PersonDtoValidator()
    {
        RuleFor(person => person.FirstName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(person => person.LastName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(person => person.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150);

        RuleFor(person => person.DateOfBirth)
            .Must(dateOfBirth =>
                dateOfBirth < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth must be in the past.");

        RuleFor(person => person.PhoneNumber)
            .MaximumLength(30)
            .When(person => person.PhoneNumber != null);
    }
}