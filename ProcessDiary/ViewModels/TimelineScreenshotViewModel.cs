using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ProcessDiary.Application.Timeline;
using ProcessDiary.Domain.Entities;
using ProcessDiary.Presentation;

namespace ProcessDiary.ViewModels;

/// <summary>A screenshot on the timeline.</summary>
public sealed partial class TimelineScreenshotViewModel : ObservableObject
{
	[ObservableProperty]
	public partial bool IsSelected { get; set; }

	[ObservableProperty]
	public partial bool IsPreviewed { get; set; }

	/// <summary>Initializes the view model.</summary>
	/// <param name="item">The screenshot with the state of its file.</param>
	/// <param name="entry">The session the screenshot belongs to.</param>
	public TimelineScreenshotViewModel(TimelineScreenshot item, TimelineEntry entry)
	{
		Screenshot = item.Screenshot;
		FileExists = item.FileExists;
		ImageSource = item.FileExists ? new BitmapImage(new Uri(item.Screenshot.FilePath)) : null;
		CapturedText = item.Screenshot.CapturedAt.ToString("HH:mm:ss");
		Details = CreateDetails(item, entry);
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

	/// <summary>Gets all known information about the screenshot and its application.</summary>
	public IReadOnlyList<ScreenshotDetail> Details { get; }

	private static List<ScreenshotDetail> CreateDetails(TimelineScreenshot item, TimelineEntry entry)
	{
		var screenshot = item.Screenshot;
		var session = entry.Session;
		var details = new List<ScreenshotDetail>
		{
			new("Captured", screenshot.CapturedAt.ToString("f")),
			new("Size", $"{screenshot.Width} × {screenshot.Height} pixels"),
		};

		if (item.FileExists)
		{
			details.Add(new("File size", FormatFileSize(new FileInfo(screenshot.FilePath).Length)));
		}

		details.Add(new("File", screenshot.FilePath));
		details.Add(new("Application", session.ProcessName));
		details.Add(new("Application path", session.ExecutablePath));
		details.Add(new("Window title", session.WindowTitle));
		details.Add(new("Session started", session.StartedAt.ToString("f")));
		details.Add(new("Session ended", session.EndedAt is { } ended ? ended.ToString("f") : "Still running"));
		details.Add(new("Session length", DurationFormatter.Format(entry.Duration)));
		return details;
	}

	private static string FormatFileSize(long bytes) => bytes switch
	{
		>= 1024 * 1024 => $"{bytes / (1024.0 * 1024.0):0.#} MB",
		>= 1024 => $"{bytes / 1024.0:0.#} KB",
		_ => $"{bytes} bytes",
	};
}
