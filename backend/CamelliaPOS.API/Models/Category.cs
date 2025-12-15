namespace CamelliaPOS.API.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty; // Emoji or icon name
    public string Color { get; set; } = "#000000";
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public List<MenuItem> MenuItems { get; set; } = new();
}

