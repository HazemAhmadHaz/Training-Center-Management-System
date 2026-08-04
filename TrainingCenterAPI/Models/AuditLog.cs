public class AuditLog
{
    public int AuditLogId { get; set; }

    public int? PersonId { get; set; }

    public string Action { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public Person? Person { get; set; }
}