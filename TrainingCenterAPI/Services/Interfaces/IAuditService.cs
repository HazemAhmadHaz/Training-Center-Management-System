using TrainingCenterAPI.Models;

namespace TrainingCenterAPI.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        int? personId,
        string action,
        string? description = null);
}