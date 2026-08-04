using System.Data;
using TrainingCenterAPI.Data;
using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Services.Security;

public static class DbInitializer
{
    public static async Task Initialize(
        TrainingCenterDbContext context,
        IPasswordHasher passwordHasher)
    {
        if (context.People.Any(x => x.Email == "admin@trainingcenter.com"))
            return;

        var admin = new Person
        {
            FirstName = "System",
            LastName = "Admin",
            Email = "admin@trainingcenter.com",
            PasswordHash = passwordHasher.HashPassword("Admin123!"),
            DateOfBirth = new DateOnly(1990, 1, 1),
            PhoneNumber = "0790000001",
            Role = UserRole.Admin
        };

        context.People.Add(admin);
        await context.SaveChangesAsync();

        context.Admins.Add(new Admin
        {
            PersonId = admin.PersonId,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }
}