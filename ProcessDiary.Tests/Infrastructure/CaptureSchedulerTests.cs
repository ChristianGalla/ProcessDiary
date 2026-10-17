using Microsoft.Extensions.Logging.Abstractions;
using ProcessDiary.Domain;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Infrastructure.Scheduling;
using ProcessDiary.Infrastructure.Settings;
using ProcessDiary.Tests.Infrastructure.Fakes;

namespace ProcessDiary.Tests.Infrastructure;

/// <summary>Tests for <see cref="CaptureScheduler"/> using in-memory fakes.</summary>
public sealed class CaptureSchedulerTests
{
    private readonly FakeForegroundWindowService _foreground = new();
    private readonly FakeScreenshotService _screenshotService = new();
    private readonly FakeProcessSessionRepository _sessions = new();
    private readonly FakeScreenshotRepository _screenshots = new();
    private readonly CaptureScheduler _scheduler;

    /// <summary>Initializes the scheduler with a fast poll interval and a long capture interval.</summary>
    public CaptureSchedulerTests()
    {
        var settings = new DiarySettingsProvider();
        settings.Update(new DiarySettings(@"C:\fake", CaptureMode.ActiveWindow, TimeSpan.FromHours(1), true, TimeSpan.FromMilliseconds(10)));
        _scheduler = new CaptureScheduler(
            _foreground, _screenshotService, new FakeScreenshotFileStore(), _sessions, _screenshots,
            settings, TimeProvider.System, NullLogger<CaptureScheduler>.Instance);
    }

    private static ForegroundWindowInfo Window(string name) => new(name, $@"C:\{name}", "title", 1);

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "Condition was not met in time.");
            await Task.Delay(10);
        }
    }

    /// <summary>After the service starts, nothing is recorded until the user starts recording.</summary>
    [Fact]
    public async Task AfterStart_DoesNotRecordUntilStartedByUser()
    {
        _foreground.Current = Window("a.exe");
        await _scheduler.StartAsync(CancellationToken.None);
        await Task.Delay(100);

        Assert.Equal(RecordingState.Disabled, _scheduler.State);
        Assert.Empty(_sessions.Items);
        Assert.Equal(0, _screenshotService.Count);
        Assert.Null(await _scheduler.CaptureNowAsync(CancellationToken.None));

        await _scheduler.StartRecordingAsync(CancellationToken.None);
        await WaitUntilAsync(() => _sessions.Items.Count == 1);
        await _scheduler.StopAsync(CancellationToken.None);
    }

    /// <summary>A foreground change starts a session and takes an activity-change screenshot.</summary>
    [Fact]
    public async Task ForegroundChange_StartsSessionAndCapturesScreenshot()
    {
        _foreground.Current = Window("code.exe");
        await _scheduler.StartAsync(CancellationToken.None);
        await _scheduler.StartRecordingAsync(CancellationToken.None);

        await WaitUntilAsync(() => _screenshots.Items.Count == 1);
        await _scheduler.StopAsync(CancellationToken.None);

        var session = Assert.Single(_sessions.Items);
        Assert.Equal("code.exe", session.ProcessName);
        Assert.Equal(session.Id, _screenshots.Items.Single().ProcessSessionId);
        Assert.False(session.IsOpen);
    }

    /// <summary>Switching applications ends the previous session and starts a new one.</summary>
    [Fact]
    public async Task SwitchingApplication_EndsPreviousSession()
    {
        _foreground.Current = Window("a.exe");
        await _scheduler.StartAsync(CancellationToken.None);
        await _scheduler.StartRecordingAsync(CancellationToken.None);
        await WaitUntilAsync(() => _sessions.Items.Count == 1);

        _foreground.Current = Window("b.exe");
        await WaitUntilAsync(() => _sessions.Items.Count == 2);
        await _scheduler.StopAsync(CancellationToken.None);

        Assert.All(_sessions.Items, s => Assert.False(s.IsOpen));
        Assert.Equal(new[] { "a.exe", "b.exe" }, _sessions.Items.Select(s => s.ProcessName).ToArray());
    }

    /// <summary>Nothing is recorded while paused, and a manual capture is refused.</summary>
    [Fact]
    public async Task Pause_StopsRecordingAndRefusesManualCapture()
    {
        _foreground.Current = Window("a.exe");
        await _scheduler.StartAsync(CancellationToken.None);
        await _scheduler.StartRecordingAsync(CancellationToken.None);
        await WaitUntilAsync(() => _sessions.Items.Count == 1);

        await _scheduler.PauseAsync(CancellationToken.None);
        _foreground.Current = Window("b.exe");
        await Task.Delay(100);

        Assert.Equal(RecordingState.Paused, _scheduler.State);
        Assert.Single(_sessions.Items);
        Assert.False(_sessions.Items[0].IsOpen);
        Assert.Null(await _scheduler.CaptureNowAsync(CancellationToken.None));

        await _scheduler.StopAsync(CancellationToken.None);
    }

    /// <summary>Resuming records the current foreground application again.</summary>
    [Fact]
    public async Task Resume_RecordsAgain()
    {
        _foreground.Current = Window("a.exe");
        await _scheduler.StartAsync(CancellationToken.None);
        await _scheduler.StartRecordingAsync(CancellationToken.None);
        await WaitUntilAsync(() => _sessions.Items.Count == 1);
        await _scheduler.PauseAsync(CancellationToken.None);

        await _scheduler.ResumeAsync(CancellationToken.None);
        await WaitUntilAsync(() => _sessions.Items.Count == 2);
        await _scheduler.StopAsync(CancellationToken.None);

        Assert.Equal(RecordingState.Recording, _scheduler.State);
    }

    /// <summary>Explorer is ignored and never starts a session.</summary>
    [Fact]
    public async Task IgnoredProcess_DoesNotStartSession()
    {
        _foreground.Current = Window("explorer.exe");
        await _scheduler.StartAsync(CancellationToken.None);
        await _scheduler.StartRecordingAsync(CancellationToken.None);
        await Task.Delay(100);
        await _scheduler.StopAsync(CancellationToken.None);

        Assert.Empty(_sessions.Items);
    }
}
