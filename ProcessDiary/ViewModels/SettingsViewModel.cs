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

    private readonly IDiarySettingsStore _store;
    private readonly IDataManagementService _data;
    private readonly IFolderPickerService _folderPicker;
    private readonly IDialogService _dialogs;
    private readonly ILogger<SettingsViewModel> _logger;

    [ObservableProperty]
    private string _screenshotFolder = string.Empty;

    [ObservableProperty]
    private double _intervalSeconds;

    [ObservableProperty]
    private CaptureModeOption _selectedMode = AllModes[0];

    [ObservableProperty]
    private bool _captureOnActivityChange;

    [ObservableProperty]
    private bool _retentionEnabled;

    [ObservableProperty]
    private double _retentionDays = 30;

    [ObservableProperty]
    private string? _message;

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

    /// <summary>Discards unsaved edits and shows the stored settings.</summary>
    public void Reload()
    {
        var current = _store.Current;
        ScreenshotFolder = current.ScreenshotFolder;
        IntervalSeconds = current.CaptureInterval.TotalSeconds;
        SelectedMode = AllModes.First(m => m.Mode == current.CaptureMode);
        CaptureOnActivityChange = current.CaptureOnActivityChange;
        RetentionEnabled = current.RetentionDays is not null;
        RetentionDays = current.RetentionDays ?? 30;
        Message = null;
    }

    [RelayCommand]
    private async Task BrowseAsync()
    {
        if (await _folderPicker.PickFolderAsync() is { } folder)
        {
            ScreenshotFolder = folder;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
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

        var settings = _store.Current with
        {
            ScreenshotFolder = ScreenshotFolder,
            CaptureMode = SelectedMode.Mode,
            CaptureInterval = TimeSpan.FromSeconds(IntervalSeconds),
            CaptureOnActivityChange = CaptureOnActivityChange,
            RetentionDays = RetentionEnabled ? (int)RetentionDays : null
        };

        try
        {
            await _store.SaveAsync(settings, CancellationToken.None);
            await _data.ApplyRetentionAsync(CancellationToken.None);
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
