namespace CamelliaPOS.API.Models;

/// <summary>
/// System-wide configuration stored locally for offline use.
/// </summary>
public class Settings
{
    public int Id { get; set; }
    public decimal TaxPercentage { get; set; } = 0;
    public string Currency { get; set; } = "LKR";
    public int SessionTimeoutMinutes { get; set; } = 30; // frontend can auto-logout after inactivity
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
}

