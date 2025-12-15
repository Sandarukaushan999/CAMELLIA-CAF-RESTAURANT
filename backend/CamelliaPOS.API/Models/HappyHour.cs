using System.Text.Json;

namespace CamelliaPOS.API.Models;

public class HappyHour
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string CategoryIdsJson { get; set; } = "[]"; // JSON array of category IDs stored as string
    public bool IsActive { get; set; } = true;
    public string DayOfWeekJson { get; set; } = "[]"; // JSON array of day of week stored as string

    // Helper properties to work with lists
    public List<int> CategoryIds
    {
        get => JsonSerializer.Deserialize<List<int>>(CategoryIdsJson) ?? new List<int>();
        set => CategoryIdsJson = JsonSerializer.Serialize(value);
    }

    public List<int> DayOfWeek
    {
        get => JsonSerializer.Deserialize<List<int>>(DayOfWeekJson) ?? new List<int>();
        set => DayOfWeekJson = JsonSerializer.Serialize(value);
    }
}

