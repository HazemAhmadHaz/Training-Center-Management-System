using TrainingCenterAPI.Enums;

namespace TrainingCenterAPI.DTOs;

public class StudentDto : PersonDto
{
    public int StudentId { get; set; }

    public DateTime RegisteredAt { get; set; }

    public StudentStatus Status { get; set; }
}

public class CreateStudentDto : PersonDto
{
    public string Password { get; set; } = null!;
}


public class UpdateStudentDto : PersonDto
{
    public StudentStatus Status { get; set; }

    public string? CurrentPassword { get; set; }

    public string? NewPassword { get; set; }
}
