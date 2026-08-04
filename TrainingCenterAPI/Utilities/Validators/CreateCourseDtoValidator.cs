using FluentValidation;
using TrainingCenterAPI.DTOs;

/// <summary>
/// Validates the shape of CreateCourseDto before the controller action
/// runs (empty fields, negative price, etc.) — synchronous only, no
/// database access, since ASP.NET Core's automatic validation pipeline
/// can't run async rules.
///
/// Principle: Single Responsibility.
/// Only checks field-level correctness. Business rules that need the
/// database (like Code uniqueness or InstructorId existence) are
/// intentionally NOT here — they live in CourseService instead, since
/// validators can't safely run async DB queries in this pipeline.
///
/// Example — if Title is sent empty, the request is rejected with a
/// 400 automatically, and Create() in CoursesController never runs:
///     RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.");
///     
/// * This is the first layer of validation, its not the first to be checked, but its the first to be shown to the client
/// </summary>

namespace TrainingCenterAPI.Utilities.Validators
{
    public class CreateCourseDtoValidator : AbstractValidator<CreateCourseDto>
    {
        public CreateCourseDtoValidator()
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