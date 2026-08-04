using FluentValidation;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Repositories.Interfaces;

/// <summary>
/// Same purpose as CreateCourseDtoValidator, but for UpdateCourseDto
/// (used on PUT requests). Includes a Status rule since updates can
/// change a course's status, unlike creation where Status always
/// defaults to Draft in the Service layer.
///
/// Principle: Single Responsibility.
/// Field-shape validation only, kept separate from CreateCourseDtoValidator
/// since Create and Update DTOs can have different rules over time.
/// </summary>

namespace TrainingCenterAPI.Utilities.Validators
{
    public class UpdateCourseDtoValidator : AbstractValidator<UpdateCourseDto>
    {
        public UpdateCourseDtoValidator(ICourseRepository courseRepository)
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(150);

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required.")
                .MaximumLength(30);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");

            RuleFor(x => x.DurationHours)
                .GreaterThan(0).WithMessage("Duration must be greater than 0.");

            RuleFor(x => x.InstructorId)
                .GreaterThan(0).WithMessage("A valid instructor must be assigned.");
        }
    }
}