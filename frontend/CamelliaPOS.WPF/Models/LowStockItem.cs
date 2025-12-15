namespace CamelliaPOS.WPF.Models;

public class LowStockItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public string Category { get; set; } = string.Empty;
}


