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
    private const int MarginDips = 16;

    private readonly IIndicatorWindowStyler _styler;

    /// <summary>Initializes the overlay.</summary>
    /// <param name="viewModel">Provides the recording state.</param>
    /// <param name="protection">Excludes the overlay from screen capture.</param>
    /// <param name="styler">Shapes and places the overlay as a dot in the screen corner.</param>
    public RecordingOverlayWindow(MainViewModel viewModel, IWindowCaptureProtection protection, IIndicatorWindowStyler styler)
    {
        ViewModel = viewModel;
        _styler = styler;
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
    }

    /// <summary>Gets the view model that provides the recording state.</summary>
    public MainViewModel ViewModel { get; }

    /// <summary>Shows the overlay as a dot in the top-right corner of the primary monitor.</summary>
    public void Show()
    {
        var handle = WindowNative.GetWindowHandle(this);

        // The shape is applied before and after activation, because showing the window can reset its size and frame.
        _styler.ApplyDotStyle(handle, DiameterDips, MarginDips);
        Activate();
        _styler.ApplyDotStyle(handle, DiameterDips, MarginDips);
    }
}
