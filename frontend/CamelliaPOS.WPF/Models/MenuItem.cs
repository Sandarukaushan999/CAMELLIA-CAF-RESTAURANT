namespace CamelliaPOS.WPF.Models;

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
    public string CategoryName { get; set; } = string.Empty;
    public bool IsCombo { get; set; }
    public bool IsActive { get; set; }
    public int StockQuantity { get; set; }
    public List<ComboItem>? ComboItems { get; set; }
}

public class ComboItem
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

