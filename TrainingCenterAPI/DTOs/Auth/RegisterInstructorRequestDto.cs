namespace TrainingCenterAPI.DTOs.Auth
{
    public class RegisterInstructorRequestDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public DateOnly HireDate { get; set; }
    }
}