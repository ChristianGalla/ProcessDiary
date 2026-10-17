using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ProcessDiary.Application.Privacy;
using ProcessDiary.Application.Timeline;
using ProcessDiary.Presentation;
using ProcessDiary.Services;

namespace ProcessDiary.ViewModels;

/// <summary>Shows the sessions and screenshots of a period and lets the user delete some of them.</summary>
public sealed partial class TimelineViewModel : ObservableObject
{
    private readonly ITimelineService _timeline;
    private readonly IDataManagementService _data;
    private readonly IDialogService _dialogs;
    private readonly ILogger<TimelineViewModel> _logger;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string? Message { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    public partial bool HasLoaded { get; set; }

    /// <summary>Initializes the view model.</summary>
    /// <param name="timeline">Source of the timeline.</param>
    /// <param name="data">Deletes sessions and screenshots.</param>
    /// <param name="dialogs">Asks for confirmation before deleting.</param>
    /// <param name="logger">The logger.</param>
    public TimelineViewModel(
        ITimelineService timeline,
        IDataManagementService data,
        IDialogService dialogs,
        ILogger<TimelineViewModel> logger)
    {
        _timeline = timeline;
        _data = data;
        _dialogs = dialogs;
        _logger = logger;
        Sessions.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));
    }

    /// <summary>Gets the period selection.</summary>
    public FilterViewModel Filter { get; } = new();

    /// <summary>Gets the sessions of the chosen period, newest first.</summary>
    public ObservableCollection<TimelineSessionViewModel> Sessions { get; } = [];

    /// <summary>Gets a value indicating whether a loaded period contains no activity.</summary>
    public bool IsEmpty => HasLoaded && Sessions.Count == 0;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (!Filter.TryCreateFilter(out var filter, out var error))
        {
            Message = error;
            return;
        }

        IsLoading = true;
        Message = null;
        try
        {
            var timeline = await _timeline.GetTimelineAsync(filter, CancellationToken.None);
            Sessions.Clear();
            foreach (var entry in timeline.Entries)
            {
                Sessions.Add(new TimelineSessionViewModel(entry));
            }

            HasLoaded = true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Timeline could not be loaded");
            Message = "The timeline could not be loaded.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        var sessions = Sessions.Where(s => s.IsSelected).ToList();
        var screenshots = Sessions
            .Where(s => !s.IsSelected)
            .SelectMany(s => s.Screenshots)
            .Where(s => s.IsSelected)
            .Select(s => s.Screenshot)
            .ToList();

        if (sessions.Count == 0 && screenshots.Count == 0)
        {
            Message = "Select sessions or screenshots to delete.";
            return;
        }

        var confirmed = await _dialogs.ConfirmAsync(
            "Delete selected items?",
            $"{sessions.Count} sessions (with their screenshots) and {screenshots.Count} single screenshots will be deleted permanently.",
            "Delete");
        if (!confirmed)
        {
            return;
        }

        string result;
        try
        {
            await _data.DeleteScreenshotsAsync(screenshots, CancellationToken.None);
            await _data.DeleteSessionsAsync(sessions.Select(s => s.Id).ToList(), CancellationToken.None);
            result = "The selected items were deleted.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Selected items could not be deleted");
            result = "Some items could not be deleted.";
        }

        await LoadAsync();
        Message = result;
    }
}
