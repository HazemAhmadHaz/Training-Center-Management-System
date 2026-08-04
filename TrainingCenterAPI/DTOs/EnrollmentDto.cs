using TrainingCenterAPI.Enums;

namespace TrainingCenterAPI.DTOs;

public class EnrollmentDto
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = null!;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = null!;
    public string CourseCode { get; set; } = null!;
    public DateTime EnrollmentDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public decimal ProgressPercent { get; set; }
    public decimal? FinalGrade { get; set; }
    public EnrollmentStatus Status { get; set; }
}

public class CreateEnrollmentDto
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
}

public class UpdateEnrollmentDto
{
    public decimal ProgressPercent { get; set; }
    public decimal? FinalGrade { get; set; }
    public EnrollmentStatus Status { get; set; }
}
