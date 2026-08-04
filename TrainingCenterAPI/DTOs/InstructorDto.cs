using TrainingCenterAPI.Enums;

namespace TrainingCenterAPI.DTOs;

public class InstructorDto : PersonDto
{
    public int InstructorId { get; set; }

    public DateOnly HireDate { get; set; }

    public decimal Salary { get; set; }

    public int? ManagerId { get; set; }

    public string? ManagerName { get; set; }

    public bool IsActive { get; set; }
}

public class CreateInstructorDto : PersonDto
{
    public DateOnly HireDate { get; set; }

    public decimal Salary { get; set; }

    public int? ManagerId { get; set; }

    public string Password { get; set; } = null!;
}

public class UpdateInstructorDto : PersonDto
{
    public DateOnly HireDate { get; set; }

    public decimal Salary { get; set; }

    public int? ManagerId { get; set; }

    public bool IsActive { get; set; }

    public string? CurrentPassword { get; set; }

    public string? NewPassword { get; set; }
}