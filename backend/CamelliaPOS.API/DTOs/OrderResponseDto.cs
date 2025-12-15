using CamelliaPOS.API.Models;

namespace CamelliaPOS.API.DTOs;

public class OrderResponseDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = new();
}

public class OrderItemResponseDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsHappyHourDiscount { get; set; }
    public decimal DiscountAmount { get; set; }
}

