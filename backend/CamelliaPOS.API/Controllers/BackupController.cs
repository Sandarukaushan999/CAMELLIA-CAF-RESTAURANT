using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CamelliaPOS.API.Services;

namespace CamelliaPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BackupController : ControllerBase
{
    private readonly BackupService _backupService;

    public BackupController(BackupService backupService)
    {
        _backupService = backupService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateBackup()
    {
        try
        {
            var backupPath = await _backupService.CreateBackupAsync();
            return Ok(new { message = "Backup created successfully", path = backupPath });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error creating backup: {ex.Message}" });
        }
    }

    [HttpGet("list")]
    public IActionResult GetBackups()
    {
        try
        {
            var backups = _backupService.GetBackupFiles();
            return Ok(backups.Select(f => new
            {
                fileName = Path.GetFileName(f),
                created = new FileInfo(f).CreationTime,
                size = new FileInfo(f).Length
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error listing backups: {ex.Message}" });
        }
    }

    [HttpPost("clean")]
    public IActionResult CleanOldBackups([FromQuery] int keepDays = 30)
    {
        try
        {
            _backupService.CleanOldBackups(keepDays);
            return Ok(new { message = "Old backups cleaned successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error cleaning backups: {ex.Message}" });
        }
    }
}

