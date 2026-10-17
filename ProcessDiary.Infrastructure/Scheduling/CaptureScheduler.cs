using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProcessDiary.Domain;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Entities;
using ProcessDiary.Domain.Sessions;

namespace ProcessDiary.Infrastructure.Scheduling;

/// <summary>
/// Records foreground sessions and takes screenshots on an interval, on activity change and on request.
/// Nothing is recorded while the state is not <see cref="RecordingState.Recording"/>.
/// </summary>
internal sealed class CaptureScheduler : BackgroundService, ICaptureScheduler
{
    private readonly IForegroundWindowService _foregroundWindow;
    private readonly IScreenshotService _screenshotService;
    private readonly IScreenshotFileStore _fileStore;
    private readonly IProcessSessionRepository _sessions;
    private readonly IScreenshotRepository _screenshots;
    private readonly IDiarySettingsProvider _settings;
    private readonly TimeProvider _time;
    private readonly ILogger<CaptureScheduler> _logger;
    private readonly SessionTransitionLogic _transitions;
    private readonly SemaphoreSlim _gate = new(1, 1);

    // Recording is opt-in: it only starts when the user asks for it.
    private RecordingState _state = RecordingState.Disabled;
    private ProcessSession? _currentSession;
    private DateTimeOffset _lastCaptureAt;

    /// <summary>Initializes the scheduler.</summary>
    /// <param name="foregroundWindow">Reads the foreground application.</param>
    /// <param name="screenshotService">Captures images.</param>
    /// <param name="fileStore">Stores image files.</param>
    /// <param name="sessions">Stores sessions.</param>
    /// <param name="screenshots">Stores screenshot records.</param>
    /// <param name="settings">Provides interval and capture settings.</param>
    /// <param name="time">Provides the current time.</param>
    /// <param name="logger">The logger.</param>
    public CaptureScheduler(
        IForegroundWindowService foregroundWindow,
        IScreenshotService screenshotService,
        IScreenshotFileStore fileStore,
        IProcessSessionRepository sessions,
        IScreenshotRepository screenshots,
        IDiarySettingsProvider settings,
        TimeProvider time,
        ILogger<CaptureScheduler> logger)
    {
        _foregroundWindow = foregroundWindow;
        _screenshotService = screenshotService;
        _fileStore = fileStore;
        _sessions = sessions;
        _screenshots = screenshots;
        _settings = settings;
        _time = time;
        _logger = logger;
        _transitions = new SessionTransitionLogic(Process.GetCurrentProcess().ProcessName);
    }

    /// <inheritdoc />
    public event EventHandler<RecordingState>? StateChanged;

    /// <inheritdoc />
    public RecordingState State => _state;

    /// <inheritdoc />
    public async Task StartRecordingAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            SetState(RecordingState.Recording);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task PauseAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_state != RecordingState.Recording)
            {
                return;
            }

            await CloseCurrentSessionAsync(cancellationToken);
            SetState(RecordingState.Paused);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task ResumeAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_state is RecordingState.Paused or RecordingState.Error)
            {
                SetState(RecordingState.Recording);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<Screenshot?> CaptureNowAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_state != RecordingState.Recording)
            {
                return null;
            }

            return await CaptureAsync(CaptureTrigger.Manual, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        await _gate.WaitAsync(CancellationToken.None);
        try
        {
            await CloseCurrentSessionAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Open session could not be closed during shutdown");
        }
        finally
        {
            _gate.Release();
            _logger.LogInformation("Recording stopped");
        }
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Recording service ready; waiting for the user to start recording");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await TickAsync(stoppingToken);
                await Task.Delay(_settings.Current.ForegroundPollInterval, _time, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_state != RecordingState.Recording)
            {
                return;
            }

            await UpdateSessionAsync(cancellationToken);

            var settings = _settings.Current;
            if (_currentSession is not null && _time.GetLocalNow() - _lastCaptureAt >= settings.CaptureInterval)
            {
                await CaptureAsync(CaptureTrigger.Interval, cancellationToken);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Without working storage nothing can be recorded, so the user must see the error state.
            _logger.LogError(ex, "Recording stopped because of a storage failure");
            SetState(RecordingState.Error);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task UpdateSessionAsync(CancellationToken cancellationToken)
    {
        var now = _time.GetLocalNow();
        var transition = _transitions.Evaluate(_currentSession, _foregroundWindow.GetCurrent(), now, Guid.NewGuid);
        if (!transition.HasChange)
        {
            return;
        }

        if (transition.Ended is { } ended)
        {
            await _sessions.UpdateAsync(ended, cancellationToken);
        }

        if (transition.Started is { } started)
        {
            await _sessions.AddAsync(started, cancellationToken);
            _currentSession = started;
            _logger.LogInformation("Session started for {ProcessName}", started.ProcessName);

            if (_settings.Current.CaptureOnActivityChange)
            {
                await CaptureAsync(CaptureTrigger.ActivityChange, cancellationToken);
            }
        }
    }

    private async Task<Screenshot?> CaptureAsync(CaptureTrigger trigger, CancellationToken cancellationToken)
    {
        if (_currentSession is not { } session)
        {
            return null;
        }

        try
        {
            var result = await _screenshotService.CaptureAsync(cancellationToken);
            var screenshot = await _fileStore.SaveAsync(result, session, cancellationToken);
            await _screenshots.AddAsync(screenshot, cancellationToken);
            _lastCaptureAt = _time.GetLocalNow();
            _logger.LogInformation("Screenshot captured ({Trigger})", trigger);
            return screenshot;
        }
        catch (Exception ex) when (ex is not OperationCanceledException and (IOException or UnauthorizedAccessException or InvalidOperationException or System.Runtime.InteropServices.COMException or TimeoutException))
        {
            // A single failed capture must not end the recording; try again at the next trigger.
            _lastCaptureAt = _time.GetLocalNow();
            _logger.LogWarning(ex, "Screenshot ({Trigger}) could not be captured", trigger);
            return null;
        }
    }

    private async Task CloseCurrentSessionAsync(CancellationToken cancellationToken)
    {
        if (SessionTransitionLogic.Close(_currentSession, _time.GetLocalNow()) is { } closed)
        {
            await _sessions.UpdateAsync(closed, cancellationToken);
            _logger.LogInformation("Session ended for {ProcessName}", closed.ProcessName);
        }

        _currentSession = null;
    }

    private void SetState(RecordingState state)
    {
        if (_state == state)
        {
            return;
        }

        _state = state;
        _logger.LogInformation("Recording state changed to {State}", state);
        StateChanged?.Invoke(this, state);
    }
}
