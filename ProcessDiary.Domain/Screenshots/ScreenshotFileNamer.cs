using System.Globalization;
using System.Text;

namespace ProcessDiary.Domain.Screenshots;

/// <summary>Builds screenshot names and folders.</summary>
public static class ScreenshotFileNamer
{
    /// <summary>Builds a file name of the form <c>{processName}_{yyyy-MM-ddTHH-mm-ss}.png</c>.</summary>
    /// <param name="processName">Process name; the extension is removed and invalid characters are replaced.</param>
    /// <param name="capturedAt">Capture time.</param>
    /// <returns>The file name.</returns>
    public static string BuildFileName(string processName, DateTimeOffset capturedAt)
    {
        var name = Path.GetFileNameWithoutExtension(processName);
        var safe = Sanitize(string.IsNullOrWhiteSpace(name) ? "desktop" : name);
        return $"{safe}_{capturedAt.ToString("yyyy-MM-dd'T'HH-mm-ss", CultureInfo.InvariantCulture)}.png";
    }

    /// <summary>Builds the folder <c>{root}\YYYY\MM\DD</c> for a capture time.</summary>
    /// <param name="rootFolder">Configured root folder.</param>
    /// <param name="capturedAt">Capture time.</param>
    /// <returns>The dated folder path.</returns>
    public static string BuildFolder(string rootFolder, DateTimeOffset capturedAt) =>
        Path.Combine(rootFolder,
            capturedAt.ToString("yyyy", CultureInfo.InvariantCulture),
            capturedAt.ToString("MM", CultureInfo.InvariantCulture),
            capturedAt.ToString("dd", CultureInfo.InvariantCulture));

    private static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            sb.Append(Array.IndexOf(invalid, c) >= 0 ? '_' : c);
        }
        return sb.ToString();
    }
}
