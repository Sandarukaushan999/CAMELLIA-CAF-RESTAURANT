using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CamelliaPOS.API.Data;
using CamelliaPOS.API.DTOs;
using CamelliaPOS.API.Models;
using CamelliaPOS.API.Services;
using System.Security.Claims;

namespace CamelliaPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuItemsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuditService _audit;

    public MenuItemsController(ApplicationDbContext context, AuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetMenuItemsByCategory(int categoryId)
    {
        var items = await _context.MenuItems
            .Include(m => m.Category)
            .Include(m => m.ComboItems)
                .ThenInclude(c => c.Item)
            .Where(m => m.CategoryId == categoryId && m.IsActive && m.ApprovalStatus == ApprovalStatus.Approved)
            .Select(m => new MenuItemDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                Cost = m.Cost,
                ImagePath = m.ImagePath,
                Barcode = m.Barcode,
                CategoryId = m.CategoryId,
                CategoryName = m.Category.Name,
                IsCombo = m.IsCombo,
                IsActive = m.IsActive,
                StockQuantity = m.StockQuantity,
                ApprovalStatus = m.ApprovalStatus,
                ComboItems = m.IsCombo ? m.ComboItems.Select(c => new ComboItemDto
                {
                    ItemId = c.ItemId,
                    ItemName = c.Item.Name,
                    Quantity = c.Quantity
                }).ToList() : null
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMenuItem(int id)
    {
        var item = await _context.MenuItems
            .Include(m => m.Category)
            .Include(m => m.ComboItems)
                .ThenInclude(c => c.Item)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null)
            return NotFound();

        var dto = new MenuItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            Cost = item.Cost,
            ImagePath = item.ImagePath,
            Barcode = item.Barcode,
            CategoryId = item.CategoryId,
            CategoryName = item.Category.Name,
            IsCombo = item.IsCombo,
            IsActive = item.IsActive,
            StockQuantity = item.StockQuantity,
            ApprovalStatus = item.ApprovalStatus,
            ComboItems = item.IsCombo ? item.ComboItems.Select(c => new ComboItemDto
            {
                ItemId = c.ItemId,
                ItemName = c.Item.Name,
                Quantity = c.Quantity
            }).ToList() : null
        };

        return Ok(dto);
    }

    [HttpGet("barcode/{barcode}")]
    public async Task<IActionResult> GetMenuItemByBarcode(string barcode)
    {
        var item = await _context.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Barcode == barcode && m.IsActive && m.ApprovalStatus == ApprovalStatus.Approved);

        if (item == null)
            return NotFound();

        var dto = new MenuItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            Cost = item.Cost,
            ImagePath = item.ImagePath,
            Barcode = item.Barcode,
            CategoryId = item.CategoryId,
            CategoryName = item.Category.Name,
            IsCombo = item.IsCombo,
            IsActive = item.IsActive,
            StockQuantity = item.StockQuantity,
            ApprovalStatus = item.ApprovalStatus
        };

        return Ok(dto);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin,Manager,Inventory")]
    public async Task<IActionResult> GetAllMenuItems()
    {
        var items = await _context.MenuItems
            .Include(m => m.Category)
            .OrderBy(m => m.Name)
            .Select(m => new MenuItemDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                Cost = m.Cost,
                ImagePath = m.ImagePath,
                Barcode = m.Barcode,
                CategoryId = m.CategoryId,
                CategoryName = m.Category.Name,
                IsCombo = m.IsCombo,
                IsActive = m.IsActive,
                StockQuantity = m.StockQuantity,
                ApprovalStatus = m.ApprovalStatus
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Inventory")]
    public async Task<IActionResult> CreateMenuItem([FromBody] CreateMenuItemDto dto)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var approvalStatus = role == "Admin" ? ApprovalStatus.Approved : ApprovalStatus.Pending;

        var menuItem = new MenuItem
        {
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            Price = dto.Price,
            Cost = dto.Cost,
            ImagePath = dto.ImagePath,
            Barcode = dto.Barcode,
            CategoryId = dto.CategoryId,
            IsCombo = dto.IsCombo,
            IsActive = true,
            StockQuantity = dto.StockQuantity,
            MinStockLevel = dto.MinStockLevel,
            ApprovalStatus = approvalStatus,
            CreatedAt = DateTime.UtcNow
        };

        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();
        await _audit.LogAsync("Create", "MenuItem", menuItem.Id, User.FindFirstValue(ClaimTypes.Name), $"Created {menuItem.Name}");

        if (dto.IsCombo && dto.ComboItems != null)
        {
            foreach (var comboItem in dto.ComboItems)
            {
                _context.ComboItems.Add(new ComboItem
                {
                    ComboId = menuItem.Id,
                    ItemId = comboItem.ItemId,
                    Quantity = comboItem.Quantity
                });
            }
            await _context.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, menuItem);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Inventory")]
    public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] UpdateMenuItemDto dto)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null)
            return NotFound();

        var role = User.FindFirstValue(ClaimTypes.Role);
        var approvalStatus = role == "Admin" ? ApprovalStatus.Approved : ApprovalStatus.Pending;

        item.Name = dto.Name;
        item.Description = dto.Description ?? string.Empty;
        item.Price = dto.Price;
        item.Cost = dto.Cost;
        item.ImagePath = dto.ImagePath;
        item.Barcode = dto.Barcode;
        item.CategoryId = dto.CategoryId;
        item.IsCombo = dto.IsCombo;
        item.StockQuantity = dto.StockQuantity;
        item.MinStockLevel = dto.MinStockLevel;
        item.ApprovalStatus = approvalStatus;
        item.UpdatedAt = DateTime.UtcNow;

        if (dto.IsCombo && dto.ComboItems != null)
        {
            var existingComboItems = _context.ComboItems.Where(c => c.ComboId == id);
            _context.ComboItems.RemoveRange(existingComboItems);

            foreach (var comboItem in dto.ComboItems)
            {
                _context.ComboItems.Add(new ComboItem
                {
                    ComboId = id,
                    ItemId = comboItem.ItemId,
                    Quantity = comboItem.Quantity
                });
            }
        }

        await _context.SaveChangesAsync();
        await _audit.LogAsync("Update", "MenuItem", item.Id, User.FindFirstValue(ClaimTypes.Name), $"Updated {item.Name}");
        return NoContent();
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetPendingItems()
    {
        var items = await _context.MenuItems
            .Include(m => m.Category)
            .Where(m => m.ApprovalStatus == ApprovalStatus.Pending)
            .Select(m => new MenuItemDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                Cost = m.Cost,
                CategoryId = m.CategoryId,
                CategoryName = m.Category.Name,
                ApprovalStatus = m.ApprovalStatus
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ApproveMenuItem(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null)
            return NotFound();

        item.ApprovalStatus = ApprovalStatus.Approved;
        await _context.SaveChangesAsync();
        await _audit.LogAsync("Approve", "MenuItem", item.Id, User.FindFirstValue(ClaimTypes.Name));

        return NoContent();
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RejectMenuItem(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null)
            return NotFound();

        item.ApprovalStatus = ApprovalStatus.Rejected;
        await _context.SaveChangesAsync();
        await _audit.LogAsync("Reject", "MenuItem", item.Id, User.FindFirstValue(ClaimTypes.Name));

        return NoContent();
    }

    [HttpPost("{id}/set-active")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SetActive(int id, [FromBody] SetActiveDto dto)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null)
            return NotFound();

        item.IsActive = dto.IsActive;
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _audit.LogAsync(dto.IsActive ? "Enable" : "Disable", "MenuItem", item.Id, User.FindFirstValue(ClaimTypes.Name));

        return Ok(new { item.Id, item.IsActive });
    }

    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin,Manager,Inventory")]
    public async Task<IActionResult> GetLowStock()
    {
        var items = await _context.MenuItems
            .Where(m => m.IsActive && m.StockQuantity <= m.MinStockLevel)
            .OrderBy(m => m.StockQuantity)
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.StockQuantity,
                m.MinStockLevel,
                Category = m.Category.Name
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("{id}/adjust-stock")]
    [Authorize(Roles = "Admin,Manager,Inventory")]
    public async Task<IActionResult> AdjustStock(int id, [FromBody] StockAdjustmentDto dto)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null)
            return NotFound();

        if (item.StockQuantity + dto.QuantityChange < 0)
            return BadRequest(new { message = "Resulting stock cannot be negative" });

        item.StockQuantity += dto.QuantityChange;
        item.UpdatedAt = DateTime.UtcNow;

        var username = User.FindFirstValue(ClaimTypes.Name);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        var log = new StockLog
        {
            MenuItemId = id,
            QuantityChange = dto.QuantityChange,
            Reason = dto.Reason,
            CreatedAt = DateTime.UtcNow,
            UserId = user?.Id
        };

        _context.StockLogs.Add(log);
        await _context.SaveChangesAsync();
        await _audit.LogAsync("AdjustStock", "MenuItem", id, username, $"Change: {dto.QuantityChange}, Reason: {dto.Reason}");

        return Ok(new { item.StockQuantity });
    }
}

public class CreateMenuItemDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public string? ImagePath { get; set; }
    public string? Barcode { get; set; }
    public int CategoryId { get; set; }
    public bool IsCombo { get; set; }
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public List<ComboItemDto>? ComboItems { get; set; }
}

public class UpdateMenuItemDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public string? ImagePath { get; set; }
    public string? Barcode { get; set; }
    public int CategoryId { get; set; }
    public bool IsCombo { get; set; }
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public List<ComboItemDto>? ComboItems { get; set; }
}

