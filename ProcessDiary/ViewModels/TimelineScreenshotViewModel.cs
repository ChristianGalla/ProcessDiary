using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ProcessDiary.Application.Timeline;
using ProcessDiary.Domain.Entities;

namespace ProcessDiary.ViewModels;

/// <summary>A screenshot on the timeline.</summary>
public sealed partial class TimelineScreenshotViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    /// <summary>Initializes the view model.</summary>
    /// <param name="item">The screenshot with the state of its file.</param>
    public TimelineScreenshotViewModel(TimelineScreenshot item)
    {
        Screenshot = item.Screenshot;
        FileExists = item.FileExists;
        ImageSource = item.FileExists ? new BitmapImage(new Uri(item.Screenshot.FilePath)) : null;
        CapturedText = item.Screenshot.CapturedAt.ToString("HH:mm:ss");
    }

    /// <summary>Gets the stored screenshot.</summary>
    public Screenshot Screenshot { get; }

    /// <summary>Gets a value indicating whether the image file still exists.</summary>
    public bool FileExists { get; }

    /// <summary>Gets a value indicating whether the image file is missing.</summary>
    public bool IsMissing => !FileExists;

    /// <summary>Gets the image, or null when the file is missing.</summary>
    public ImageSource? ImageSource { get; }

    /// <summary>Gets the time of day the screenshot was taken.</summary>
    public string CapturedText { get; }
}
