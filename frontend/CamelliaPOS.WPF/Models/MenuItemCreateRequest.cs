namespace CamelliaPOS.WPF.Models;

public class MenuItemCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public string? ImagePath { get; set; }
    public string? Barcode { get; set; }
    public int CategoryId { get; set; }
    public bool IsCombo { get; set; }
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public List<ComboItem>? ComboItems { get; set; }
}


