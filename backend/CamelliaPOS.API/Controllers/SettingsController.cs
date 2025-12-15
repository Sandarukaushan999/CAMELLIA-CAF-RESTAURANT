using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CamelliaPOS.API.Data;
using CamelliaPOS.API.DTOs;
using CamelliaPOS.API.Services;

namespace CamelliaPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SettingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuditService _audit;

    public SettingsController(ApplicationDbContext context, AuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var settings = await _context.Settings.FirstOrDefaultAsync(s => s.Id == 1);
        if (settings == null)
        {
            return NotFound();
        }

        return Ok(new SettingsDto
        {
            TaxPercentage = settings.TaxPercentage,
            Currency = settings.Currency,
            SessionTimeoutMinutes = settings.SessionTimeoutMinutes
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateSettingsDto dto)
    {
        var settings = await _context.Settings.FirstOrDefaultAsync(s => s.Id == 1);
        if (settings == null)
        {
            return NotFound();
        }

        settings.TaxPercentage = dto.TaxPercentage;
        settings.Currency = dto.Currency;
        settings.SessionTimeoutMinutes = dto.SessionTimeoutMinutes;
        settings.UpdatedAt = DateTime.UtcNow;
        settings.UpdatedBy = User.FindFirstValue(ClaimTypes.Name);

        await _context.SaveChangesAsync();
        await _audit.LogAsync("Update", "Settings", settings.Id, settings.UpdatedBy, "Updated tax/currency/session timeout");

        return NoContent();
    }

    [HttpPost("change-credentials")]
    public async Task<IActionResult> ChangeCredentials([FromBody] ChangePasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest(new { message = "Passwords are required" });

        var username = User.FindFirstValue(ClaimTypes.Name);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return Unauthorized();

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return BadRequest(new { message = "Current password is incorrect" });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();
        await _audit.LogAsync("Update", "User", user.Id, username, "Changed password");

        return Ok(new { message = "Credentials updated" });
    }

    [HttpPost("reset")]
    public async Task<IActionResult> ResetSystem()
    {
        // Soft reset: clear transactional data but keep inventory and users
        _context.OrderItems.RemoveRange(_context.OrderItems);
        _context.Orders.RemoveRange(_context.Orders);
        _context.Wastes.RemoveRange(_context.Wastes);
        _context.StockLogs.RemoveRange(_context.StockLogs);

        await _context.SaveChangesAsync();
        await _audit.LogAsync("Reset", "System", null, User.FindFirstValue(ClaimTypes.Name), "Cleared transactional data");

        return Ok(new { message = "System reset completed" });
    }
}

