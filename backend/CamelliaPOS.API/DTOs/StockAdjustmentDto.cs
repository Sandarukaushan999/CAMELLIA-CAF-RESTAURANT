namespace CamelliaPOS.API.DTOs;

public class StockAdjustmentDto
{
    public int QuantityChange { get; set; }
    public string Reason { get; set; } = string.Empty;
}

