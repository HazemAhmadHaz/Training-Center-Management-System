using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Models;

public class Person
{
    public int PersonId { get; set; }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    = new List<RefreshToken>();


}