using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CamelliaPOS.API.Data;
using CamelliaPOS.API.Models;
using CamelliaPOS.API.Services;

namespace CamelliaPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuditService _audit;

    public CategoriesController(ApplicationDbContext context, AuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Icon,
                c.Color,
                c.DisplayOrder
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Name is required" });

        var category = new Category
        {
            Name = dto.Name,
            Icon = dto.Icon ?? string.Empty,
            Color = dto.Color ?? "#8B4513",
            DisplayOrder = dto.DisplayOrder,
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        await _audit.LogAsync("Create", "Category", category.Id, User.Identity?.Name, $"Created {category.Name}");

        return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        category.Name = dto.Name ?? category.Name;
        category.Icon = dto.Icon ?? category.Icon;
        category.Color = dto.Color ?? category.Color;
        category.DisplayOrder = dto.DisplayOrder ?? category.DisplayOrder;
        category.IsActive = dto.IsActive ?? category.IsActive;

        await _context.SaveChangesAsync();
        await _audit.LogAsync("Update", "Category", category.Id, User.Identity?.Name, $"Updated {category.Name}");

        return NoContent();
    }
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int DisplayOrder { get; set; } = 1;
}

public class UpdateCategoryDto
{
    public string? Name { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
}

