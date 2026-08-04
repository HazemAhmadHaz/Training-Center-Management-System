namespace TrainingCenterAPI.DTOs;

public class StudentProfileDto
{
    public int StudentId { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Bio { get; set; }
    public string? LinkedInUrl { get; set; }
}

public class CreateStudentProfileDto
{
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Bio { get; set; }
    public string? LinkedInUrl { get; set; }
}

public class UpdateStudentProfileDto
{
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Bio { get; set; }
    public string? LinkedInUrl { get; set; }
}
