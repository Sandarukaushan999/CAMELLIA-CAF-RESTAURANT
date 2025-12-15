namespace CamelliaPOS.API.Models;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public bool IsHappyHour { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
}

public enum PaymentMethod
{
    Cash = 0,
    Card = 1
}

