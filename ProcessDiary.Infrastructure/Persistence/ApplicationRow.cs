namespace ProcessDiary.Infrastructure.Persistence;

/// <summary>Row of the Applications table; one entry per distinct process name.</summary>
internal sealed class ApplicationRow
{
    /// <summary>Gets or sets the primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the process name (unique, case-insensitive).</summary>
    public string ProcessName { get; set; } = string.Empty;

    /// <summary>Gets or sets the friendly name, if the user or the app provided one.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Gets or sets the optional report category.</summary>
    public string? Category { get; set; }

    /// <summary>Gets or sets the sessions of this application.</summary>
    public List<SessionRow> Sessions { get; set; } = [];
}
