using CamelliaPOS.API.Models;

namespace CamelliaPOS.API.DTOs;

public class OrderDto
{
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal PaidAmount { get; set; }
}

public class OrderItemDto
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
}

