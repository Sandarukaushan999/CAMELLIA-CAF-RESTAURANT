using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CamelliaPOS.API.Data;
using CamelliaPOS.API.DTOs;
using CamelliaPOS.API.Models;
using System.Security.Claims;

namespace CamelliaPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderDto orderDto)
    {
        // Check if there are pending inventory changes
        var hasPendingChanges = await _context.MenuItems
            .AnyAsync(m => m.ApprovalStatus == ApprovalStatus.Pending);

        if (hasPendingChanges)
        {
            return BadRequest(new { message = "Cannot process orders. There are pending inventory changes that need approval." });
        }

        var username = User.FindFirstValue(ClaimTypes.Name);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return Unauthorized();

        var currentTime = DateTime.UtcNow;
        var timeOnly = TimeOnly.FromDateTime(currentTime);
        var dayOfWeek = (int)currentTime.DayOfWeek;

        // Check for happy hour
        var happyHour = await _context.HappyHours
            .FirstOrDefaultAsync(h => h.IsActive && 
                h.DayOfWeek.Contains(dayOfWeek) &&
                timeOnly >= h.StartTime && 
                timeOnly <= h.EndTime);

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            OrderDate = currentTime,
            UserId = user.Id,
            PaymentMethod = orderDto.PaymentMethod,
            IsHappyHour = happyHour != null,
            OrderItems = new List<OrderItem>()
        };

        decimal subtotal = 0;

        foreach (var itemDto in orderDto.Items)
        {
            var menuItem = await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == itemDto.MenuItemId && m.IsActive && m.ApprovalStatus == ApprovalStatus.Approved);

            if (menuItem == null)
                return BadRequest(new { message = $"MenuItem {itemDto.MenuItemId} not found or not approved" });

            if (menuItem.StockQuantity < itemDto.Quantity)
                return BadRequest(new { message = $"Insufficient stock for {menuItem.Name}" });

            decimal unitPrice = menuItem.Price;
            decimal discountAmount = 0;
            bool isHappyHourDiscount = false;

            // Apply happy hour discount
            if (happyHour != null && happyHour.CategoryIds.Contains(menuItem.CategoryId))
            {
                discountAmount = unitPrice * (happyHour.DiscountPercentage / 100);
                unitPrice = unitPrice - discountAmount;
                isHappyHourDiscount = true;
            }

            // Handle combo items
            if (menuItem.IsCombo)
            {
                var comboItems = await _context.ComboItems
                    .Include(c => c.Item)
                    .Where(c => c.ComboId == menuItem.Id)
                    .ToListAsync();

                foreach (var comboItem in comboItems)
                {
                    if (comboItem.Item.StockQuantity < comboItem.Quantity * itemDto.Quantity)
                        return BadRequest(new { message = $"Insufficient stock for combo item {comboItem.Item.Name}" });
                }
            }

            var totalPrice = unitPrice * itemDto.Quantity;

            order.OrderItems.Add(new OrderItem
            {
                MenuItemId = menuItem.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice,
                IsHappyHourDiscount = isHappyHourDiscount,
                DiscountAmount = discountAmount * itemDto.Quantity
            });

            subtotal += totalPrice;

            // Update stock
            if (!menuItem.IsCombo)
            {
                menuItem.StockQuantity -= itemDto.Quantity;
            }
            else
            {
                var comboItems = await _context.ComboItems
                    .Include(c => c.Item)
                    .Where(c => c.ComboId == menuItem.Id)
                    .ToListAsync();

                foreach (var comboItem in comboItems)
                {
                    comboItem.Item.StockQuantity -= comboItem.Quantity * itemDto.Quantity;
                }
            }
        }

        order.Subtotal = subtotal;
        order.Discount = orderDto.Discount;
        order.Tax = orderDto.Tax;
        order.Total = subtotal - orderDto.Discount + orderDto.Tax;
        order.PaidAmount = orderDto.PaidAmount;
        order.Balance = orderDto.PaidAmount - order.Total;

        if (order.Balance < 0)
            return BadRequest(new { message = "Paid amount is less than total amount" });

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var response = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.Id == order.Id)
            .Select(o => new OrderResponseDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                Subtotal = o.Subtotal,
                Discount = o.Discount,
                Tax = o.Tax,
                Total = o.Total,
                PaidAmount = o.PaidAmount,
                Balance = o.Balance,
                PaymentMethod = o.PaymentMethod,
                Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ItemName = oi.MenuItem.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    IsHappyHourDiscount = oi.IsHappyHourDiscount,
                    DiscountAmount = oi.DiscountAmount
                }).ToList()
            })
            .FirstOrDefaultAsync();

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.Id == id)
            .Select(o => new OrderResponseDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                Subtotal = o.Subtotal,
                Discount = o.Discount,
                Tax = o.Tax,
                Total = o.Total,
                PaidAmount = o.PaidAmount,
                Balance = o.Balance,
                PaymentMethod = o.PaymentMethod,
                Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ItemName = oi.MenuItem.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    IsHappyHourDiscount = oi.IsHappyHourDiscount,
                    DiscountAmount = oi.DiscountAmount
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (order == null)
            return NotFound();

        return Ok(order);
    }

    private string GenerateOrderNumber()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = _context.Orders.Count(o => o.OrderDate.Date == DateTime.UtcNow.Date) + 1;
        return $"ORD-{date}-{count:D4}";
    }
}

