namespace CamelliaPOS.WPF.Models;

public class Settings
{
    public decimal TaxPercentage { get; set; }
    public string Currency { get; set; } = "LKR";
    public int SessionTimeoutMinutes { get; set; }
}


