namespace CamelliaPOS.API.Models;

public class Waste
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty; // Expired, Spoiled, Kitchen Waste
    public decimal Cost { get; set; }
    public DateTime WasteDate { get; set; } = DateTime.UtcNow;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}

