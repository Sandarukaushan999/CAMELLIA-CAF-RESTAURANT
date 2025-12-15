namespace CamelliaPOS.API.Models;

public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public string? ImagePath { get; set; }
    public string? Barcode { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public bool IsCombo { get; set; }
    public bool IsActive { get; set; } = true;
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Approved;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public List<ComboItem> ComboItems { get; set; } = new();
    public List<OrderItem> OrderItems { get; set; } = new();
}

public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

