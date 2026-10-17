using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Sessions;

namespace ProcessDiary.Tests;

/// <summary>Tests for <see cref="SessionTransitionLogic"/>.</summary>
public class SessionTransitionLogicTests
{
    private static readonly DateTimeOffset T0 = new(2026, 1, 5, 9, 0, 0, TimeSpan.Zero);
    private readonly SessionTransitionLogic _logic = new("ProcessDiary.exe");

    private static ForegroundWindowInfo Fg(string name) => new(name, $@"C:\{name}", "title", 1);

    [Fact]
    public void FirstForegroundWindow_StartsSession()
    {
        var t = _logic.Evaluate(null, Fg("code.exe"), T0, Guid.NewGuid);
        Assert.Null(t.Ended);
        Assert.Equal("code.exe", t.Started!.ProcessName);
        Assert.Equal(T0, t.Started.StartedAt);
    }

    [Fact]
    public void ChangedApplication_EndsPreviousAndStartsNew()
    {
        var first = _logic.Evaluate(null, Fg("code.exe"), T0, Guid.NewGuid).Started!;
        var t = _logic.Evaluate(first, Fg("chrome.exe"), T0.AddMinutes(5), Guid.NewGuid);
        Assert.Equal(TimeSpan.FromMinutes(5), t.Ended!.Duration);
        Assert.Equal("chrome.exe", t.Started!.ProcessName);
    }

    [Fact]
    public void SameApplication_NoChange()
    {
        var first = _logic.Evaluate(null, Fg("code.exe"), T0, Guid.NewGuid).Started!;
        Assert.False(_logic.Evaluate(first, Fg("CODE.EXE"), T0.AddMinutes(1), Guid.NewGuid).HasChange);
    }

    [Theory]
    [InlineData("explorer.exe")]
    [InlineData("Explorer")]
    [InlineData("ProcessDiary.exe")]
    [InlineData("ProcessDiary")]
    [InlineData("")]
    public void IgnoredProcesses_DoNotChangeSessions(string name)
    {
        var first = _logic.Evaluate(null, Fg("code.exe"), T0, Guid.NewGuid).Started!;
        Assert.False(_logic.Evaluate(first, Fg(name), T0.AddMinutes(1), Guid.NewGuid).HasChange);
        Assert.False(_logic.Evaluate(null, Fg(name), T0, Guid.NewGuid).HasChange);
    }

    [Fact]
    public void NullForeground_NoChange()
    {
        Assert.False(_logic.Evaluate(null, null, T0, Guid.NewGuid).HasChange);
    }
}
