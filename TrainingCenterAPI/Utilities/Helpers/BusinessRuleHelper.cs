using TrainingCenterAPI.Utilities.Exceptions;

/// <summary>
/// Reusable helper for throwing BusinessRuleException when a
/// business rule is violated, instead of writing "if (...) throw new
/// BusinessRuleException(...)" by hand in every Service method.
///
/// Principle: DRY + Single Responsibility.
/// Used across CourseService (and future StudentService,
/// InstructorService, etc.) for any "reject if X" or "reject if not X"
/// business check.
///
/// Example:
///     var codeExists = await _courseRepository.CodeExistsAsync(dto.Code);
///     BusinessRuleHelper.ThrowIfExists(codeExists, $"Course code '{dto.Code}' already exists.");
/// </summary>

namespace TrainingCenterAPI.Utilities.Helpers
{
    public static class BusinessRuleHelper
    {
        public static void ThrowIfExists(bool condition, string message)
        {
            if (condition)
            {
                throw new BusinessRuleException(message);
            }
        }

        public static void ThrowIfNotExists(bool condition, string message)
        {
            if (!condition)
            {
                throw new BusinessRuleException(message);
            }
        }
    }
}