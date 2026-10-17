using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using ProcessDiary.Domain;
using Windows.UI;

namespace ProcessDiary.Presentation;

/// <summary>Maps a recording state to the text and colors shown by every recording indicator.</summary>
public static class StatusPresentation
{
    private static readonly SolidColorBrush RecordingBrush = new(Color.FromArgb(255, 209, 52, 56));
    private static readonly SolidColorBrush PausedBrush = new(Color.FromArgb(255, 247, 181, 0));
    private static readonly SolidColorBrush DisabledBrush = new(Colors.Gray);
    private static readonly SolidColorBrush ErrorBrush = new(Color.FromArgb(255, 142, 0, 0));

    /// <summary>Gets the text that describes a state.</summary>
    /// <param name="state">The recording state.</param>
    /// <returns>A short, user-friendly description.</returns>
    public static string GetText(RecordingState state) => state switch
    {
        RecordingState.Recording => "Recording",
        RecordingState.Paused => "Paused",
        RecordingState.Disabled => "Not recording",
        RecordingState.Error => "Recording stopped because of an error",
        _ => string.Empty
    };

    /// <summary>Gets the color of a state.</summary>
    /// <param name="state">The recording state.</param>
    /// <returns>A brush; red for recording, amber for paused, gray for disabled, dark red for an error.</returns>
    public static SolidColorBrush GetBrush(RecordingState state) => state switch
    {
        RecordingState.Recording => RecordingBrush,
        RecordingState.Paused => PausedBrush,
        RecordingState.Error => ErrorBrush,
        _ => DisabledBrush
    };

    /// <summary>Gets the label of the command that changes the state.</summary>
    /// <param name="state">The recording state.</param>
    /// <returns>The text of the primary recording action.</returns>
    public static string GetActionText(RecordingState state) => state switch
    {
        RecordingState.Recording => "Pause recording",
        RecordingState.Paused => "Resume recording",
        RecordingState.Error => "Try again",
        _ => "Start recording"
    };
}
