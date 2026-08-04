namespace TrainingCenterAPI.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }
}