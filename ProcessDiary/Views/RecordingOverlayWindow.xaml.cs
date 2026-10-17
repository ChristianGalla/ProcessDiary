using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.ViewModels;
using Windows.Graphics;
using WinRT.Interop;

namespace ProcessDiary.Views;

/// <summary>
/// A small always-on-top status window that shows the recording state on the desktop.
/// It is excluded from screenshots, so it never appears in the diary.
/// </summary>
public sealed partial class RecordingOverlayWindow : Window
{
    private const int WidthPixels = 260;
    private const int HeightPixels = 40;
    private const int MarginPixels = 16;

    /// <summary>Initializes the overlay.</summary>
    /// <param name="viewModel">Provides the recording state.</param>
    /// <param name="protection">Excludes the overlay from screen capture.</param>
    public RecordingOverlayWindow(MainViewModel viewModel, IWindowCaptureProtection protection)
    {
        ViewModel = viewModel;
        InitializeComponent();

        var presenter = OverlappedPresenter.Create();
        presenter.IsAlwaysOnTop = true;
        presenter.IsResizable = false;
        presenter.IsMaximizable = false;
        presenter.IsMinimizable = false;
        presenter.SetBorderAndTitleBar(false, false);
        AppWindow.SetPresenter(presenter);
        AppWindow.IsShownInSwitchers = false;

        var workArea = DisplayArea.Primary.WorkArea;
        AppWindow.MoveAndResize(new RectInt32(
            workArea.X + workArea.Width - WidthPixels - MarginPixels,
            workArea.Y + MarginPixels,
            WidthPixels,
            HeightPixels));

        protection.ExcludeFromCapture(WindowNative.GetWindowHandle(this));
    }

    /// <summary>Gets the view model that provides the recording state.</summary>
    public MainViewModel ViewModel { get; }

    /// <summary>Shows the overlay without taking the focus from the user's application.</summary>
    public void ShowWithoutActivation() => AppWindow.Show(false);
}
