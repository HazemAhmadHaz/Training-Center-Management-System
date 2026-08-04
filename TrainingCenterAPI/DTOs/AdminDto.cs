namespace TrainingCenterAPI.DTOs;

public class AdminDto : PersonDto
{
    public int AdminId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAdminDto : PersonDto
{
    public string Password { get; set; } = null!;
}

public class UpdateAdminDto : PersonDto
{
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}