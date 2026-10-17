namespace ProcessDiary.Infrastructure.Persistence;

/// <summary>Row of the Sessions table.</summary>
internal sealed class SessionRow
{
    /// <summary>Gets or sets the primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the foreign key of the application.</summary>
    public Guid ApplicationId { get; set; }

    /// <summary>Gets or sets the application navigation.</summary>
    public ApplicationRow Application { get; set; } = null!;

    /// <summary>Gets or sets the full executable path.</summary>
    public string ExecutablePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the window title at the start of the session.</summary>
    public string WindowTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the start time as UTC ticks (stored as integer so it can be compared in queries).</summary>
    public long StartedAtUtcTicks { get; set; }

    /// <summary>Gets or sets the end time as UTC ticks, or null while the session is open.</summary>
    public long? EndedAtUtcTicks { get; set; }

    /// <summary>Gets or sets the screenshots of this session.</summary>
    public List<ScreenshotRow> Screenshots { get; set; } = [];
}
