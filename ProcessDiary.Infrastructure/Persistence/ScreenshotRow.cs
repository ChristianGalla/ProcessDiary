namespace ProcessDiary.Infrastructure.Persistence;

/// <summary>Row of the Screenshots table.</summary>
internal sealed class ScreenshotRow
{
    /// <summary>Gets or sets the primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the foreign key of the session.</summary>
    public Guid SessionId { get; set; }

    /// <summary>Gets or sets the session navigation.</summary>
    public SessionRow Session { get; set; } = null!;

    /// <summary>Gets or sets the image file path.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the capture time as UTC ticks.</summary>
    public long CapturedAtUtcTicks { get; set; }

    /// <summary>Gets or sets the image width in pixels.</summary>
    public int Width { get; set; }

    /// <summary>Gets or sets the image height in pixels.</summary>
    public int Height { get; set; }
}
