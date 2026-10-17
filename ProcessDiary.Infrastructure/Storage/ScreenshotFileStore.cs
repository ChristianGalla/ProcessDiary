using Microsoft.Extensions.Logging;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Entities;
using ProcessDiary.Domain.Screenshots;

namespace ProcessDiary.Infrastructure.Storage;

/// <summary>Stores screenshot files in dated folders below the configured root.</summary>
internal sealed class ScreenshotFileStore : IScreenshotFileStore
{
    private readonly IDiarySettingsProvider _settings;
    private readonly ILogger<ScreenshotFileStore> _logger;

    /// <summary>Initializes the store.</summary>
    /// <param name="settings">Provides the configured screenshot folder.</param>
    /// <param name="logger">The logger.</param>
    public ScreenshotFileStore(IDiarySettingsProvider settings, ILogger<ScreenshotFileStore> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Screenshot> SaveAsync(ScreenshotResult result, ProcessSession session, CancellationToken cancellationToken)
    {
        var folder = ScreenshotFileNamer.BuildFolder(_settings.Current.ScreenshotFolder, result.CapturedAt);
        var path = Path.Combine(folder, ScreenshotFileNamer.BuildFileName(session.ProcessName, result.CapturedAt));

        try
        {
            Directory.CreateDirectory(folder);
            path = MakeUnique(path);
            await File.WriteAllBytesAsync(path, result.PngData, cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Screenshot could not be stored in {Folder}", folder);
            throw;
        }

        _logger.LogInformation("Screenshot stored for {ProcessName} ({Width}x{Height})",
            session.ProcessName, result.Width, result.Height);
        return new Screenshot(Guid.NewGuid(), session.Id, path, result.CapturedAt, result.Width, result.Height);
    }

    /// <inheritdoc />
    public bool Exists(Screenshot screenshot) => File.Exists(screenshot.FilePath);

    /// <inheritdoc />
    public Task<int> DeleteFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken)
    {
        var deleted = 0;
        foreach (var path in filePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    deleted++;
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(ex, "Screenshot file {FileName} could not be deleted", Path.GetFileName(path));
            }
        }

        return Task.FromResult(deleted);
    }

    // Two captures within the same second would otherwise overwrite each other.
    private static string MakeUnique(string path)
    {
        if (!File.Exists(path))
        {
            return path;
        }

        var directory = Path.GetDirectoryName(path)!;
        var name = Path.GetFileNameWithoutExtension(path);
        for (var i = 1; ; i++)
        {
            var candidate = Path.Combine(directory, $"{name}_{i}.png");
            if (!File.Exists(candidate))
            {
                return candidate;
            }
        }
    }
}
