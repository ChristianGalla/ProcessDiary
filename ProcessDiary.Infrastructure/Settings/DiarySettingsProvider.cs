using ProcessDiary.Domain.Abstractions;

namespace ProcessDiary.Infrastructure.Settings;

/// <summary>Holds the diary settings in memory, starting from sensible defaults.</summary>
internal sealed class DiarySettingsProvider : IDiarySettingsProvider
{
    private volatile DiarySettings _current = CreateDefaults();

    /// <inheritdoc />
    public DiarySettings Current => _current;

    /// <summary>Replaces the current settings.</summary>
    /// <param name="settings">The new settings.</param>
    public void Update(DiarySettings settings) => _current = settings;

    /// <summary>Creates the default settings.</summary>
    /// <returns>Defaults: pictures folder, active window, one screenshot per minute, capture on activity change.</returns>
    public static DiarySettings CreateDefaults() => new(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "ProcessDiary"),
        CaptureMode.ActiveWindow,
        TimeSpan.FromMinutes(1),
        CaptureOnActivityChange: true,
        ForegroundPollInterval: TimeSpan.FromSeconds(1));
}
