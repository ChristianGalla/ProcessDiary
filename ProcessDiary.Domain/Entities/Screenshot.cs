namespace ProcessDiary.Domain.Entities;

/// <summary>A stored screenshot that belongs to a session.</summary>
/// <param name="Id">Unique identifier of the screenshot.</param>
/// <param name="ProcessSessionId">Identifier of the session the screenshot was taken in.</param>
/// <param name="FilePath">Full path of the image file.</param>
/// <param name="CapturedAt">When the screenshot was captured.</param>
/// <param name="Width">Image width in pixels.</param>
/// <param name="Height">Image height in pixels.</param>
public sealed record Screenshot(
    Guid Id,
    Guid ProcessSessionId,
    string FilePath,
    DateTimeOffset CapturedAt,
    int Width,
    int Height);
