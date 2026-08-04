using TrainingCenterAPI.Data;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Services.Interfaces;

namespace TrainingCenterAPI.Services.Implementations;

public class AuditService : IAuditService
{
    private readonly TrainingCenterDbContext _context;

    public AuditService(
        TrainingCenterDbContext context)
    {
        _context = context;
    }


    public async Task LogAsync(
        int? personId,
        string action,
        string? description = null)
    {
        var audit = new AuditLog
        {
            PersonId = personId,

            Action = action,

            Description = description,

            CreatedAt = DateTime.UtcNow
        };


        await _context.AuditLogs.AddAsync(audit);

        await _context.SaveChangesAsync();
    }
}