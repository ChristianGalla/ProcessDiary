namespace ProcessDiary.Domain.Abstractions;

/// <summary>Provides the diary settings and persists changes made by the user.</summary>
public interface IDiarySettingsStore : IDiarySettingsProvider
{
    /// <summary>Occurs after the settings were replaced.</summary>
    event EventHandler? Changed;

    /// <summary>Saves new settings and makes them current.</summary>
    /// <param name="settings">The settings to save.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the settings are saved.</returns>
    Task SaveAsync(DiarySettings settings, CancellationToken cancellationToken);
}
