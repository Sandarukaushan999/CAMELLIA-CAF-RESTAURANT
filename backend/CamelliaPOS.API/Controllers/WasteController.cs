using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CamelliaPOS.API.Data;
using CamelliaPOS.API.Models;
using System.Security.Claims;

namespace CamelliaPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WasteController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WasteController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Inventory")]
    public async Task<IActionResult> RecordWaste([FromBody] WasteDto dto)
    {
        var username = User.FindFirstValue(ClaimTypes.Name);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return Unauthorized();

        var menuItem = await _context.MenuItems.FindAsync(dto.MenuItemId);
        if (menuItem == null)
            return NotFound(new { message = "MenuItem not found" });

        if (menuItem.StockQuantity < dto.Quantity)
            return BadRequest(new { message = "Insufficient stock" });

        var waste = new Waste
        {
            MenuItemId = dto.MenuItemId,
            Quantity = dto.Quantity,
            Reason = dto.Reason,
            Cost = menuItem.Cost * dto.Quantity,
            WasteDate = DateTime.UtcNow,
            UserId = user.Id
        };

        menuItem.StockQuantity -= dto.Quantity;

        _context.Wastes.Add(waste);
        await _context.SaveChangesAsync();

        return Ok(new { id = waste.Id, message = "Waste recorded successfully" });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetWaste([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var query = _context.Wastes
            .Include(w => w.MenuItem)
            .Include(w => w.User)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(w => w.WasteDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(w => w.WasteDate <= endDate.Value);

        var wastes = await query
            .Select(w => new
            {
                w.Id,
                ItemName = w.MenuItem.Name,
                w.Quantity,
                w.Reason,
                w.Cost,
                w.WasteDate,
                RecordedBy = w.User.Username
            })
            .OrderByDescending(w => w.WasteDate)
            .ToListAsync();

        return Ok(wastes);
    }
}

public class WasteDto
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}

