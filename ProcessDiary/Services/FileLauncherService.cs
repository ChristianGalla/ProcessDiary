using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ProcessDiary.Services;

/// <summary>Opens files through the Windows shell.</summary>
public sealed class FileLauncherService : IFileLauncherService
{
    private readonly ILogger<FileLauncherService> _logger;

    /// <summary>Initializes the service.</summary>
    /// <param name="logger">The logger.</param>
    public FileLauncherService(ILogger<FileLauncherService> logger) => _logger = logger;

    /// <inheritdoc />
    public bool OpenFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true })?.Dispose();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "File could not be opened");
            return false;
        }
    }

    /// <inheritdoc />
    public bool ShowInFolder(string filePath)
    {
        try
        {
            var startInfo = File.Exists(filePath)
                ? new ProcessStartInfo("explorer.exe", $"/select,\"{filePath}\"")
                : Directory.Exists(Path.GetDirectoryName(filePath))
                    ? new ProcessStartInfo("explorer.exe", $"\"{Path.GetDirectoryName(filePath)}\"")
                    : null;
            if (startInfo is null)
            {
                return false;
            }

            Process.Start(startInfo)?.Dispose();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "File location could not be opened");
            return false;
        }
    }
}
