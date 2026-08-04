using System;
using System.Collections.Generic;
using TrainingCenterAPI.Enums;

namespace TrainingCenterAPI.Models;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrollmentDate { get; set; }

    public DateTime? CompletionDate { get; set; }

    public decimal ProgressPercent { get; set; }

    public decimal? FinalGrade { get; set; }

    public EnrollmentStatus Status { get; set; }
    public virtual Course Course { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
