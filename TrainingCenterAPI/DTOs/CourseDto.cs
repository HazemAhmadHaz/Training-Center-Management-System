/// <summary>
/// Never expose the raw EF Core Course entity directly to the client.
/// The entity has navigation properties (Instructor, Enrollments) that
/// risk circular references in JSON, and exposes internal DB structure
/// you may not want public.
/// </summary>

using TrainingCenterAPI.Enums;

namespace TrainingCenterAPI.DTOs
{

    /// <summary>
    /// CourseDto — shape returned to the client (GET requests).
    /// Includes InstructorName (a friendly computed field),
    /// not the full nested Instructor object.
    /// </summary>

    public class CourseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public CourseLevel Level { get; set; }
        public int DurationHours { get; set; }
        public CourseStatus Status { get; set; }
        public int InstructorId { get; set; }
        public string? InstructorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    /// <summary>
    /// CreateCourseDto — shape the client sends on POST.
    /// Excludes CourseId (DB-generated) and Status (server decides it's Draft on
    /// creation — business rule, not client's choice).
    /// </summary>

    public class CreateCourseDto
    {
        public string Title { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public CourseLevel Level { get; set; }
        public int DurationHours { get; set; }
        public int InstructorId { get; set; }
    }

    /// <summary>
    /// UpdateCourseDto — shape the client sends on PUT.
    /// Includes Status since updates can change it (e.g., publish a course).
    /// </summary>

    public class UpdateCourseDto
    {
        public string Title { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public CourseLevel Level { get; set; }
        public int DurationHours { get; set; }
        public CourseStatus Status { get; set; }
        public int InstructorId { get; set; }
    }
}