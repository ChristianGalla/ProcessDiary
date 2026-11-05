using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ProcessDiary.ViewModels;

namespace ProcessDiary.Views;

/// <summary>Shows sessions and screenshots on a timeline.</summary>
public sealed partial class TimelinePage : Page
{
    private const double MinPreviewWidth = 280;
    private const double MinTimelineWidth = 320;

    private bool _isResizing;

    /// <summary>Initializes the page.</summary>
    /// <param name="viewModel">The view model of the page.</param>
    public TimelinePage(TimelineViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    /// <summary>Gets the view model of the page.</summary>
    public TimelineViewModel ViewModel { get; }

    private void OnScreenshotClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is TimelineScreenshotViewModel { FileExists: true } screenshot)
        {
            ViewModel.PreviewScreenshot = screenshot;
        }
    }

    private void OnGripPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _isResizing = ((UIElement)sender).CapturePointer(e.Pointer);
    }

    private void OnGripPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isResizing)
        {
            return;
        }

        var grip = (FrameworkElement)sender;
        var x = e.GetCurrentPoint(this).Position.X;
        var maxWidth = Math.Max(MinPreviewWidth, ActualWidth - MinTimelineWidth - grip.ActualWidth);
        PreviewPanel.Width = Math.Clamp(ActualWidth - x - (grip.ActualWidth / 2), MinPreviewWidth, maxWidth);
    }

    private void OnGripPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isResizing = false;
        ((UIElement)sender).ReleasePointerCapture(e.Pointer);
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => ViewModel.LoadCommand.Execute(null);
}
