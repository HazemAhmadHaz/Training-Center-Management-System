using System;
using System.Collections.Generic;
using TrainingCenterAPI.Enums;

namespace TrainingCenterAPI.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public DateTime RegisteredAt { get; set; }

    public StudentStatus Status { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual StudentProfile? StudentProfile { get; set; }
    public int PersonId { get; set; }
    public virtual Person Person { get; set; } = null!;
}
