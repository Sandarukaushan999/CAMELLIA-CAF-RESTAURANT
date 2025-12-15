namespace CamelliaPOS.WPF.Models;

public class OrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public int PaymentMethod { get; set; } // 0 = Cash, 1 = Card
    public decimal PaidAmount { get; set; }
}

public class OrderItemRequest
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
}

public class OrderResponse
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
    public int PaymentMethod { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
}

public class OrderItemResponse
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsHappyHourDiscount { get; set; }
    public decimal DiscountAmount { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class CartItem
{
    public MenuItem MenuItem { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public bool IsHappyHourDiscount { get; set; }
    public decimal DiscountAmount { get; set; }
}

