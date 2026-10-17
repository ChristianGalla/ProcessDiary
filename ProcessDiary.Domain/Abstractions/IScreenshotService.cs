namespace ProcessDiary.Domain.Abstractions;

/// <summary>Captures screenshots.</summary>
public interface IScreenshotService
{
    /// <summary>Captures a screenshot using the configured capture mode.</summary>
    /// <param name="cancellationToken">Token used to cancel the capture.</param>
    /// <returns>The captured image.</returns>
    Task<ScreenshotResult> CaptureAsync(CancellationToken cancellationToken);
}
