using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.ViewModels;
using WinRT.Interop;

namespace ProcessDiary.Views;

/// <summary>
/// A small always-on-top status window that shows the recording state on the desktop.
/// It is excluded from screenshots, so it never appears in the diary.
/// </summary>
public sealed partial class RecordingOverlayWindow : Window
{
    private const int DiameterDips = 16;

    private static readonly TimeSpan KeepOnTopInterval = TimeSpan.FromMilliseconds(500);

    private readonly IIndicatorWindowStyler _styler;
    private readonly IDiarySettingsStore _settings;
    private DispatcherQueueTimer? _keepOnTopTimer;

    /// <summary>Initializes the overlay.</summary>
    /// <param name="viewModel">Provides the recording state.</param>
    /// <param name="protection">Excludes the overlay from screen capture.</param>
    /// <param name="styler">Shapes and places the overlay as a dot in the screen corner.</param>
    /// <param name="settings">Provides the current overlay position settings.</param>
    public RecordingOverlayWindow(
        MainViewModel viewModel,
        IWindowCaptureProtection protection,
        IIndicatorWindowStyler styler,
        IDiarySettingsStore settings)
    {
        ViewModel = viewModel;
        _styler = styler;
        _settings = settings;
        InitializeComponent();

        var presenter = OverlappedPresenter.Create();
        presenter.IsAlwaysOnTop = true;
        presenter.IsResizable = false;
        presenter.IsMaximizable = false;
        presenter.IsMinimizable = false;
        presenter.SetBorderAndTitleBar(false, false);
        AppWindow.SetPresenter(presenter);
        AppWindow.IsShownInSwitchers = false;

        protection.ExcludeFromCapture(WindowNative.GetWindowHandle(this));
        _settings.Changed += OnSettingsChanged;
    }

    /// <summary>Gets the view model that provides the recording state.</summary>
    public MainViewModel ViewModel { get; }

    /// <summary>Shows the overlay as a dot in the configured corner of the primary monitor.</summary>
    public void Show()
    {
        var handle = WindowNative.GetWindowHandle(this);

        // The shape is applied before and after activation, because showing the window can reset its size and frame.
        ApplyPosition(handle);
        Activate();
        ApplyPosition(handle);

        if (_keepOnTopTimer is null)
        {
            _keepOnTopTimer = DispatcherQueue.CreateTimer();
            _keepOnTopTimer.Interval = KeepOnTopInterval;
            _keepOnTopTimer.Tick += (_, _) => _styler.BringToTop(handle);
        }

        _keepOnTopTimer.Start();
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        DispatcherQueue.TryEnqueue(() => ApplyPosition(WindowNative.GetWindowHandle(this)));
    }

    private void ApplyPosition(nint handle)
    {
        var current = _settings.Current;
        _styler.ApplyDotStyle(
            handle,
            DiameterDips,
            current.OverlayCorner,
            current.OverlayHorizontalMargin,
            current.OverlayVerticalMargin);
    }
}
