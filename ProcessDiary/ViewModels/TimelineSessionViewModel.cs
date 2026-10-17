using CommunityToolkit.Mvvm.ComponentModel;
using ProcessDiary.Application.Timeline;
using ProcessDiary.Presentation;

namespace ProcessDiary.ViewModels;

/// <summary>A session on the timeline with its screenshots.</summary>
public sealed partial class TimelineSessionViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    /// <summary>Initializes the view model.</summary>
    /// <param name="entry">The timeline entry to show.</param>
    public TimelineSessionViewModel(TimelineEntry entry)
    {
        Id = entry.Session.Id;
        ApplicationName = entry.Session.ProcessName;
        WindowTitle = entry.Session.WindowTitle;
        var end = entry.Session.EndedAt is { } ended ? ended.ToString("HH:mm:ss") : "now";
        TimeText = $"{entry.Session.StartedAt:d MMM HH:mm:ss} – {end}";
        DurationText = DurationFormatter.Format(entry.Duration);
        Screenshots = entry.Screenshots.Select(s => new TimelineScreenshotViewModel(s)).ToList();
    }

    /// <summary>Gets the identifier of the session.</summary>
    public Guid Id { get; }

    /// <summary>Gets the name of the application.</summary>
    public string ApplicationName { get; }

    /// <summary>Gets the window title at the start of the session.</summary>
    public string WindowTitle { get; }

    /// <summary>Gets the start and end of the session.</summary>
    public string TimeText { get; }

    /// <summary>Gets the length of the session.</summary>
    public string DurationText { get; }

    /// <summary>Gets the screenshots taken during the session.</summary>
    public IReadOnlyList<TimelineScreenshotViewModel> Screenshots { get; }
}
