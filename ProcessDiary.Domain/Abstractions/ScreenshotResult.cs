namespace ProcessDiary.Domain.Abstractions;

/// <summary>Raw capture output.</summary>
/// <param name="PngData">PNG encoded image bytes. Must never be logged.</param>
/// <param name="Width">Image width in pixels.</param>
/// <param name="Height">Image height in pixels.</param>
/// <param name="CapturedAt">When the image was captured.</param>
public sealed record ScreenshotResult(byte[] PngData, int Width, int Height, DateTimeOffset CapturedAt);
