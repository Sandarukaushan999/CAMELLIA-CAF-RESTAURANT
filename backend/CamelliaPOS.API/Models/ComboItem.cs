namespace CamelliaPOS.API.Models;

public class ComboItem
{
    public int Id { get; set; }
    public int ComboId { get; set; }
    public MenuItem Combo { get; set; } = null!;
    public int ItemId { get; set; }
    public MenuItem Item { get; set; } = null!;
    public int Quantity { get; set; } = 1;
}

