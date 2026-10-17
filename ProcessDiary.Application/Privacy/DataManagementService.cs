using Microsoft.Extensions.Logging;
using ProcessDiary.Domain;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Application.Privacy;

/// <summary>Deletes recorded data. Recording is paused while data that may be in use is removed.</summary>
public sealed class DataManagementService : IDataManagementService
{
    private readonly IProcessSessionRepository _sessions;
    private readonly IScreenshotRepository _screenshots;
    private readonly IScreenshotFileStore _fileStore;
    private readonly ICaptureScheduler _scheduler;
    private readonly IDiarySettingsProvider _settings;
    private readonly TimeProvider _time;
    private readonly ILogger<DataManagementService> _logger;

    /// <summary>Initializes the service.</summary>
    /// <param name="sessions">Stores sessions.</param>
    /// <param name="screenshots">Stores screenshot records.</param>
    /// <param name="fileStore">Removes image files.</param>
    /// <param name="scheduler">Paused while open sessions could be deleted.</param>
    /// <param name="settings">Provides the retention period.</param>
    /// <param name="time">Provides the current time.</param>
    /// <param name="logger">The logger.</param>
    public DataManagementService(
        IProcessSessionRepository sessions,
        IScreenshotRepository screenshots,
        IScreenshotFileStore fileStore,
        ICaptureScheduler scheduler,
        IDiarySettingsProvider settings,
        TimeProvider time,
        ILogger<DataManagementService> logger)
    {
        _sessions = sessions;
        _screenshots = screenshots;
        _fileStore = fileStore;
        _scheduler = scheduler;
        _settings = settings;
        _time = time;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task DeleteAllAsync(CancellationToken cancellationToken) =>
        RunWhilePausedAsync(async () =>
        {
            // Screenshots are never captured in the future, so one day ahead covers every record.
            var all = await _screenshots.GetCapturedBeforeAsync(_time.GetLocalNow().AddDays(1), cancellationToken);
            await _fileStore.DeleteFilesAsync(all.Select(s => s.FilePath), cancellationToken);
            await _screenshots.DeleteAllAsync(cancellationToken);
            await _sessions.DeleteAllAsync(cancellationToken);
            _logger.LogInformation("All diary data deleted");
        }, cancellationToken);

    /// <inheritdoc />
    public async Task DeleteScreenshotsAsync(IReadOnlyCollection<Screenshot> screenshots, CancellationToken cancellationToken)
    {
        if (screenshots.Count == 0)
        {
            return;
        }

        await _fileStore.DeleteFilesAsync(screenshots.Select(s => s.FilePath), cancellationToken);
        await _screenshots.DeleteAsync(screenshots.Select(s => s.Id).ToList(), cancellationToken);
        _logger.LogInformation("{Count} screenshots deleted", screenshots.Count);
    }

    /// <inheritdoc />
    public Task DeleteSessionsAsync(IReadOnlyCollection<Guid> sessionIds, CancellationToken cancellationToken)
    {
        if (sessionIds.Count == 0)
        {
            return Task.CompletedTask;
        }

        return RunWhilePausedAsync(async () =>
        {
            foreach (var id in sessionIds)
            {
                var screenshots = await _screenshots.GetBySessionAsync(id, cancellationToken);
                await _fileStore.DeleteFilesAsync(screenshots.Select(s => s.FilePath), cancellationToken);
            }

            // Screenshot records are removed by the cascading delete of their session.
            await _sessions.DeleteAsync(sessionIds, cancellationToken);
            _logger.LogInformation("{Count} sessions deleted", sessionIds.Count);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task ApplyRetentionAsync(CancellationToken cancellationToken)
    {
        if (_settings.Current.RetentionDays is not { } days)
        {
            return;
        }

        var cutoff = _time.GetLocalNow().AddDays(-days);
        var old = await _screenshots.GetCapturedBeforeAsync(cutoff, cancellationToken);
        await DeleteScreenshotsAsync(old, cancellationToken);
        await _sessions.DeleteEndedBeforeAsync(cutoff, cancellationToken);
        _logger.LogInformation("Retention applied for data older than {Days} days", days);
    }

    private async Task RunWhilePausedAsync(Func<Task> action, CancellationToken cancellationToken)
    {
        var wasRecording = _scheduler.State == RecordingState.Recording;
        if (wasRecording)
        {
            await _scheduler.PauseAsync(cancellationToken);
        }

        try
        {
            await action();
        }
        finally
        {
            if (wasRecording)
            {
                await _scheduler.ResumeAsync(CancellationToken.None);
            }
        }
    }
}
