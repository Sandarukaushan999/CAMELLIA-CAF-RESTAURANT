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
public class MenuItemsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MenuItemsController(ApplicationDbContext context)
    {
        _context = context;
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

        return NoContent();
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

