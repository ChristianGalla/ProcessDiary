using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ProcessDiary.Application.Privacy;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Services;

namespace ProcessDiary.ViewModels;

/// <summary>Edits the settings and offers the privacy actions.</summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    private static readonly IReadOnlyList<CaptureModeOption> AllModes =
    [
        new(CaptureMode.ActiveWindow, "Active window"),
        new(CaptureMode.WholeDesktop, "Whole desktop")
    ];

    private static readonly IReadOnlyList<OverlayCornerOption> AllOverlayCorners =
    [
        new(OverlayCorner.TopLeft, "Top left"),
        new(OverlayCorner.TopRight, "Top right"),
        new(OverlayCorner.BottomLeft, "Bottom left"),
        new(OverlayCorner.BottomRight, "Bottom right")
    ];

    private readonly IDiarySettingsStore _store;
    private readonly IDataManagementService _data;
    private readonly IFolderPickerService _folderPicker;
    private readonly IDialogService _dialogs;
    private readonly ILogger<SettingsViewModel> _logger;
    private readonly SemaphoreSlim _saveGate = new(1, 1);
    private bool _loading;

    [ObservableProperty]
    public partial string ScreenshotFolder { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double IntervalSeconds { get; set; }

    [ObservableProperty]
    public partial CaptureModeOption SelectedMode { get; set; } = AllModes[0];

    [ObservableProperty]
    public partial bool CaptureOnActivityChange { get; set; }

    private OverlayCornerOption _selectedOverlayCorner = AllOverlayCorners[1];
    private double _overlayHorizontalMargin = 16;
    private double _overlayVerticalMargin = 30;

    /// <summary>Gets or sets the selected indicator corner.</summary>
    public OverlayCornerOption SelectedOverlayCorner
    {
        get => _selectedOverlayCorner;
        set
        {
            if (SetProperty(ref _selectedOverlayCorner, value))
            {
                QueueSave();
            }
        }
    }

    /// <summary>Gets or sets the horizontal indicator margin.</summary>
    public double OverlayHorizontalMargin
    {
        get => _overlayHorizontalMargin;
        set
        {
            if (SetProperty(ref _overlayHorizontalMargin, value))
            {
                QueueSave();
            }
        }
    }

    /// <summary>Gets or sets the vertical indicator margin.</summary>
    public double OverlayVerticalMargin
    {
        get => _overlayVerticalMargin;
        set
        {
            if (SetProperty(ref _overlayVerticalMargin, value))
            {
                QueueSave();
            }
        }
    }

    [ObservableProperty]
    public partial bool RetentionEnabled { get; set; }

    [ObservableProperty]
    public partial double RetentionDays { get; set; } = 30;

    [ObservableProperty]
    public partial string? Message { get; set; }

    /// <summary>Initializes the view model and loads the current settings.</summary>
    /// <param name="store">Stores the settings.</param>
    /// <param name="data">Deletes recorded data.</param>
    /// <param name="folderPicker">Lets the user choose the screenshot folder.</param>
    /// <param name="dialogs">Asks for confirmation before deleting.</param>
    /// <param name="logger">The logger.</param>
    public SettingsViewModel(
        IDiarySettingsStore store,
        IDataManagementService data,
        IFolderPickerService folderPicker,
        IDialogService dialogs,
        ILogger<SettingsViewModel> logger)
    {
        _store = store;
        _data = data;
        _folderPicker = folderPicker;
        _dialogs = dialogs;
        _logger = logger;
        Reload();
    }

    /// <summary>Gets the selectable capture modes.</summary>
    public IReadOnlyList<CaptureModeOption> Modes => AllModes;

    /// <summary>Gets the selectable overlay corners.</summary>
    public IReadOnlyList<OverlayCornerOption> OverlayCorners => AllOverlayCorners;

    /// <summary>Shows the stored settings.</summary>
    public void Reload()
    {
        var current = _store.Current;
        _loading = true;
        try
        {
            ScreenshotFolder = current.ScreenshotFolder;
            IntervalSeconds = current.CaptureInterval.TotalSeconds;
            SelectedMode = AllModes.First(m => m.Mode == current.CaptureMode);
            CaptureOnActivityChange = current.CaptureOnActivityChange;
            SelectedOverlayCorner = AllOverlayCorners.First(c => c.Corner == current.OverlayCorner);
            OverlayHorizontalMargin = current.OverlayHorizontalMargin;
            OverlayVerticalMargin = current.OverlayVerticalMargin;
            RetentionEnabled = current.RetentionDays is not null;
            RetentionDays = current.RetentionDays ?? 30;
            Message = null;
        }
        finally
        {
            _loading = false;
        }
    }

    partial void OnScreenshotFolderChanged(string value) => QueueSave();

    partial void OnIntervalSecondsChanged(double value) => QueueSave();

    partial void OnSelectedModeChanged(CaptureModeOption value) => QueueSave();

    partial void OnCaptureOnActivityChangeChanged(bool value) => QueueSave();

    partial void OnRetentionEnabledChanged(bool value) => QueueSave();

    partial void OnRetentionDaysChanged(double value) => QueueSave();

    private void QueueSave()
    {
        if (!_loading)
        {
            _ = SaveAsync();
        }
    }

    [RelayCommand]
    private async Task BrowseAsync()
    {
        if (await _folderPicker.PickFolderAsync() is { } folder)
        {
            ScreenshotFolder = folder;
        }
    }

    private async Task SaveAsync()
    {
        await _saveGate.WaitAsync();
        try
        {
            await SaveCoreAsync();
        }
        finally
        {
            _saveGate.Release();
        }
    }

    private async Task SaveCoreAsync()
    {
        if (!Path.IsPathRooted(ScreenshotFolder))
        {
            Message = "Choose a full folder path for the screenshots.";
            return;
        }

        if (double.IsNaN(IntervalSeconds) || IntervalSeconds < 5)
        {
            Message = "The screenshot interval must be at least 5 seconds.";
            return;
        }

        if (RetentionEnabled && (double.IsNaN(RetentionDays) || RetentionDays < 1))
        {
            Message = "Keep data for at least 1 day.";
            return;
        }

        if (!double.IsFinite(OverlayHorizontalMargin) || OverlayHorizontalMargin < 0
            || !double.IsFinite(OverlayVerticalMargin) || OverlayVerticalMargin < 0)
        {
            Message = "Overlay margins cannot be negative.";
            return;
        }

        var previousRetention = _store.Current.RetentionDays;
        var settings = _store.Current with
        {
            ScreenshotFolder = ScreenshotFolder,
            CaptureMode = SelectedMode.Mode,
            CaptureInterval = TimeSpan.FromSeconds(IntervalSeconds),
            CaptureOnActivityChange = CaptureOnActivityChange,
            RetentionDays = RetentionEnabled ? (int)RetentionDays : null,
            OverlayCorner = SelectedOverlayCorner.Corner,
            OverlayHorizontalMargin = OverlayHorizontalMargin,
            OverlayVerticalMargin = OverlayVerticalMargin
        };

        try
        {
            await _store.SaveAsync(settings, CancellationToken.None);
            if (settings.RetentionDays != previousRetention)
            {
                await _data.ApplyRetentionAsync(CancellationToken.None);
            }

            Message = "Settings saved.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Settings could not be saved");
            Message = "The settings could not be saved.";
        }
    }

    [RelayCommand]
    private async Task DeleteAllAsync()
    {
        var confirmed = await _dialogs.ConfirmAsync(
            "Delete all data?",
            "All sessions and screenshots, including the image files, will be deleted permanently.",
            "Delete everything");
        if (!confirmed)
        {
            return;
        }

        try
        {
            await _data.DeleteAllAsync(CancellationToken.None);
            Message = "All data was deleted.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Data could not be deleted");
            Message = "Some data could not be deleted.";
        }
    }
}
