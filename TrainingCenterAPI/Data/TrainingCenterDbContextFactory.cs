using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TrainingCenterAPI.Data;

public class TrainingCenterDbContextFactory
    : IDesignTimeDbContextFactory<TrainingCenterDbContext>
{
    public TrainingCenterDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<TrainingCenterDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=DESKTOP-75K1F0M;Database=TrainingCenterDB;Integrated Security=True;TrustServerCertificate=True");

        return new TrainingCenterDbContext(optionsBuilder.Options);
    }
}