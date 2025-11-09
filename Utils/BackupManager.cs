// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace LocalizationManager.Utils;

/// <summary>
/// Manages automatic backups of resource files before modifications.
/// </summary>
public class BackupManager
{
    private const int MaxBackupsToKeep = 10;

    /// <summary>
    /// Creates a timestamped backup of a file.
    /// </summary>
    /// <param name="filePath">Path to the file to back up.</param>
    /// <returns>Path to the created backup file.</returns>
    public string CreateBackup(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var directory = Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("Invalid file path");
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        var backupDir = Path.Combine(directory, ".backups");
        Directory.CreateDirectory(backupDir);

        var backupFileName = $"{fileName}.{timestamp}{extension}";
        var backupPath = Path.Combine(backupDir, backupFileName);

        File.Copy(filePath, backupPath, overwrite: false);

        // Clean up old backups
        CleanupOldBackups(backupDir, fileName, extension);

        return backupPath;
    }

    /// <summary>
    /// Creates backups for multiple files.
    /// </summary>
    /// <param name="filePaths">Collection of file paths to back up.</param>
    /// <returns>Dictionary mapping original paths to backup paths.</returns>
    public Dictionary<string, string> CreateBackups(IEnumerable<string> filePaths)
    {
        var backups = new Dictionary<string, string>();

        foreach (var filePath in filePaths)
        {
            try
            {
                var backupPath = CreateBackup(filePath);
                backups[filePath] = backupPath;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create backup for '{filePath}': {ex.Message}", ex);
            }
        }

        return backups;
    }

    /// <summary>
    /// Removes old backup files, keeping only the most recent ones.
    /// </summary>
    private void CleanupOldBackups(string backupDir, string fileName, string extension)
    {
        try
        {
            var pattern = $"{fileName}.*{extension}";
            var backupFiles = Directory.GetFiles(backupDir, pattern)
                .OrderByDescending(f => File.GetCreationTime(f))
                .ToList();

            // Keep only the most recent backups
            var filesToDelete = backupFiles.Skip(MaxBackupsToKeep);

            foreach (var file in filesToDelete)
            {
                File.Delete(file);
            }
        }
        catch
        {
            // Ignore cleanup errors - not critical
        }
    }

    /// <summary>
    /// Gets the backup directory for a resource path.
    /// </summary>
    public string GetBackupDirectory(string resourcePath)
    {
        return Path.Combine(resourcePath, ".backups");
    }

    /// <summary>
    /// Lists all backups for a specific file.
    /// </summary>
    public List<string> ListBackups(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("Invalid file path");
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        var backupDir = Path.Combine(directory, ".backups");

        if (!Directory.Exists(backupDir))
        {
            return new List<string>();
        }

        var pattern = $"{fileName}.*{extension}";
        return Directory.GetFiles(backupDir, pattern)
            .OrderByDescending(f => File.GetCreationTime(f))
            .ToList();
    }
}
