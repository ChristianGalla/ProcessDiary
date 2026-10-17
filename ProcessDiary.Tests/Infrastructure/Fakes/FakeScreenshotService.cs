using ProcessDiary.Domain.Abstractions;

namespace ProcessDiary.Tests.Infrastructure.Fakes;

/// <summary>Screenshot service that returns a tiny fixed image and counts captures.</summary>
internal sealed class FakeScreenshotService : IScreenshotService
{
    private int _count;

    /// <summary>Gets the number of captures taken.</summary>
    public int Count => _count;

    /// <inheritdoc />
    public Task<ScreenshotResult> CaptureAsync(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _count);
        return Task.FromResult(new ScreenshotResult([1], 1, 1, DateTimeOffset.Now));
    }
}
