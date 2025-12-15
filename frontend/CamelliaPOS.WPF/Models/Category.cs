namespace CamelliaPOS.WPF.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";
    public int DisplayOrder { get; set; }
}

