namespace TrainingCenterAPI.Models
{
    public partial class Instructor
    {
        public string? PasswordHash { get; set; }
        public bool IsAdmin { get; set; }
    }
}