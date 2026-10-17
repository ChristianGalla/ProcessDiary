using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Application.Timeline;

/// <summary>A screenshot together with the state of its image file.</summary>
/// <param name="Screenshot">The stored screenshot record.</param>
/// <param name="FileExists">False when the image file was removed outside of ProcessDiary.</param>
public sealed record TimelineScreenshot(Screenshot Screenshot, bool FileExists);
