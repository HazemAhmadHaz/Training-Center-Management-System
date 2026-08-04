using System;
using System.Collections.Generic;
using TrainingCenterAPI.Enums;

namespace TrainingCenterAPI.Models;

public partial class Instructor
{
    public int InstructorId { get; set; }

    public DateOnly HireDate { get; set; }

    public decimal Salary { get; set; }

    public int? ManagerId { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Instructor> InverseManager { get; set; } = new List<Instructor>();

    public virtual Instructor? Manager { get; set; }
    public int PersonId { get; set; }
    public virtual Person Person { get; set; } = null!;
}
