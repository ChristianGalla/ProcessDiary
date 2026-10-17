namespace ProcessDiary.Domain.Abstractions;

/// <summary>Provides the current diary settings.</summary>
public interface IDiarySettingsProvider
{
    /// <summary>Gets the current settings.</summary>
    DiarySettings Current { get; }
}
