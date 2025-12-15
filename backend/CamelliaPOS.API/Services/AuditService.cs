using CamelliaPOS.API.Data;
using CamelliaPOS.API.Models;

namespace CamelliaPOS.API.Services;

public class AuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string action, string entity, int? entityId, string? username, string details = "")
    {
        var log = new AuditLog
        {
            Action = action,
            Entity = entity,
            EntityId = entityId,
            Username = username,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}

