namespace TrainingCenterAPI.DTOs;

public class PersonDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
}