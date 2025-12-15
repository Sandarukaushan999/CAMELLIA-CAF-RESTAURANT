using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CamelliaPOS.API.Data;
using CamelliaPOS.API.DTOs;
using CamelliaPOS.API.Services;
using BCrypt.Net;

namespace CamelliaPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly JwtService _jwtService;

    public AuthController(ApplicationDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { message = "Username and password are required" });
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        var token = _jwtService.GenerateToken(user.Username, user.Role);

        return Ok(new LoginResponse
        {
            Token = token,
            Username = user.Username,
            Role = user.Role,
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        });
    }
}

