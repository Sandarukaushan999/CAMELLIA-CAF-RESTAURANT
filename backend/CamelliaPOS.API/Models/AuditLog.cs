namespace CamelliaPOS.API.Models;

/// <summary>
/// Simple audit trail for admin actions.
/// </summary>
public class AuditLog
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? Username { get; set; }
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

