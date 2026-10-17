namespace ProcessDiary.Domain.Abstractions;

/// <summary>User-configurable settings that influence recording and capturing.</summary>
/// <param name="ScreenshotFolder">Root folder in which screenshots are stored.</param>
/// <param name="CaptureMode">Area covered by each screenshot.</param>
/// <param name="CaptureInterval">Time between interval screenshots.</param>
/// <param name="CaptureOnActivityChange">Whether a screenshot is taken when the foreground application changes.</param>
/// <param name="ForegroundPollInterval">How often the foreground application is checked.</param>
/// <param name="RetentionDays">Number of days data is kept; older data is deleted automatically. Null keeps data until the user deletes it.</param>
public sealed record DiarySettings(
    string ScreenshotFolder,
    CaptureMode CaptureMode,
    TimeSpan CaptureInterval,
    bool CaptureOnActivityChange,
    TimeSpan ForegroundPollInterval,
    int? RetentionDays = null);
