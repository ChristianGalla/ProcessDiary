using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ProcessDiary.Domain;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Presentation;
using ProcessDiary.Services;

namespace ProcessDiary.ViewModels;

/// <summary>State and commands of the main window, the tray icon and the overlay.</summary>
public sealed partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly ICaptureScheduler _scheduler;
    private readonly IUiDispatcher _dispatcher;
    private readonly INavigationService _navigation;
    private readonly IApplicationExitService _exit;
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(PrimaryActionText))]
    [NotifyPropertyChangedFor(nameof(CanCapture))]
    private RecordingState _state;

    /// <summary>Initializes the view model and starts following the recording state.</summary>
    /// <param name="scheduler">Controls recording.</param>
    /// <param name="dispatcher">Moves state changes to the UI thread.</param>
    /// <param name="navigation">Switches pages.</param>
    /// <param name="exit">Closes the application.</param>
    /// <param name="logger">The logger.</param>
    public MainViewModel(
        ICaptureScheduler scheduler,
        IUiDispatcher dispatcher,
        INavigationService navigation,
        IApplicationExitService exit,
        ILogger<MainViewModel> logger)
    {
        _scheduler = scheduler;
        _dispatcher = dispatcher;
        _navigation = navigation;
        _exit = exit;
        _logger = logger;
        _state = scheduler.State;
        _scheduler.StateChanged += OnSchedulerStateChanged;
    }

    /// <summary>Gets the description of the recording state.</summary>
    public string StatusText => StatusPresentation.GetText(State);

    /// <summary>Gets the label of the command that starts, pauses or resumes recording.</summary>
    public string PrimaryActionText => StatusPresentation.GetActionText(State);

    /// <summary>Gets a value indicating whether a screenshot can be taken on request.</summary>
    public bool CanCapture => State == RecordingState.Recording;

    /// <summary>Navigates to a page.</summary>
    /// <param name="key">One of <see cref="NavigationKeys"/>.</param>
    public void Navigate(string key) => _navigation.Navigate(key);

    /// <inheritdoc />
    public void Dispose() => _scheduler.StateChanged -= OnSchedulerStateChanged;

    [RelayCommand]
    private async Task PrimaryActionAsync()
    {
        try
        {
            switch (State)
            {
                case RecordingState.Recording:
                    await _scheduler.PauseAsync(CancellationToken.None);
                    break;
                case RecordingState.Paused:
                case RecordingState.Error:
                    await _scheduler.ResumeAsync(CancellationToken.None);
                    break;
                default:
                    await _scheduler.StartRecordingAsync(CancellationToken.None);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Recording state could not be changed");
        }
    }

    [RelayCommand(CanExecute = nameof(CanCapture))]
    private async Task CaptureNowAsync()
    {
        try
        {
            await _scheduler.CaptureNowAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Screenshot on request failed");
        }
    }

    [RelayCommand]
    private Task ExitAsync() => _exit.ExitAsync();

    partial void OnStateChanged(RecordingState value) => CaptureNowCommand.NotifyCanExecuteChanged();

    private void OnSchedulerStateChanged(object? sender, RecordingState state) =>
        _dispatcher.Post(() => State = state);
}
