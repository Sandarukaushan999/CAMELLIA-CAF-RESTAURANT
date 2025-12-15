using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;

namespace CamelliaPOS.API.Services;

public class BackupService
{
    private readonly string _backupDirectory;
    private readonly string _databasePath;

    public BackupService(string databasePath, string? backupDirectory = null)
    {
        _databasePath = databasePath;
        _backupDirectory = backupDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
        
        // Ensure backup directory exists
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }
    }

    public async Task<string> CreateBackupAsync()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"camellia_pos_backup_{timestamp}.db";
        var backupPath = Path.Combine(_backupDirectory, backupFileName);

        // Copy database file
        if (File.Exists(_databasePath))
        {
            File.Copy(_databasePath, backupPath, true);
        }

        // Create zip archive
        var zipFileName = $"camellia_pos_backup_{timestamp}.zip";
        var zipPath = Path.Combine(_backupDirectory, zipFileName);

        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            zip.CreateEntryFromFile(backupPath, backupFileName);
        }

        // Delete uncompressed backup
        File.Delete(backupPath);

        return zipPath;
    }

    public async Task RestoreBackupAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("Backup file is empty");

        var tempPath = Path.Combine(_backupDirectory, $"restore_{Guid.NewGuid()}.zip");
        using (var stream = new FileStream(tempPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        using (var zip = ZipFile.OpenRead(tempPath))
        {
            var entry = zip.Entries.FirstOrDefault(e => e.FullName.EndsWith(".db"));
            if (entry == null)
                throw new InvalidOperationException("Backup archive does not contain a database file");

            entry.ExtractToFile(_databasePath, overwrite: true);
        }

        File.Delete(tempPath);
    }

    public void CleanOldBackups(int keepDays = 30)
    {
        var files = Directory.GetFiles(_backupDirectory, "*.zip");
        var cutoffDate = DateTime.Now.AddDays(-keepDays);

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            if (fileInfo.CreationTime < cutoffDate)
            {
                File.Delete(file);
            }
        }
    }

    public List<string> GetBackupFiles()
    {
        return Directory.GetFiles(_backupDirectory, "*.zip")
            .OrderByDescending(f => new FileInfo(f).CreationTime)
            .ToList();
    }
}

