namespace CamelliaPOS.API.Models;

/// <summary>
/// Tracks manual stock adjustments with reason and actor for auditing.
/// </summary>
public class StockLog
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public int QuantityChange { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UserId { get; set; }
    public User? User { get; set; }
}

