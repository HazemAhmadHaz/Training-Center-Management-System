namespace TrainingCenterAPI.DTOs.Auth
{
    public class RegisterRequestDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public DateOnly DateOfBirth { get; set; }
    }
}