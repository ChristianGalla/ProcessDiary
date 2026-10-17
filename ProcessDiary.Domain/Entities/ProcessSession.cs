namespace ProcessDiary.Domain.Entities;

/// <summary>A continuous period during which one application was in the foreground.</summary>
/// <param name="Id">Unique identifier of the session.</param>
/// <param name="ProcessName">Name of the process, for example <c>code.exe</c>.</param>
/// <param name="ExecutablePath">Full path of the process executable.</param>
/// <param name="WindowTitle">Title of the foreground window when the session started.</param>
/// <param name="StartedAt">When the session started.</param>
/// <param name="EndedAt">When the session ended, or null while it is still open.</param>
public sealed record ProcessSession(
    Guid Id,
    string ProcessName,
    string ExecutablePath,
    string WindowTitle,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt = null)
{
    /// <summary>Gets a value indicating whether the session has not ended yet.</summary>
    public bool IsOpen => EndedAt is null;

    /// <summary>Gets the elapsed time; zero while the session is open (use <see cref="DurationAt"/> for open sessions).</summary>
    public TimeSpan Duration => EndedAt is { } end ? end - StartedAt : TimeSpan.Zero;

    /// <summary>Gets the elapsed time, treating an open session as running until <paramref name="now"/>.</summary>
    /// <param name="now">The reference time used for open sessions.</param>
    /// <returns>The session duration.</returns>
    public TimeSpan DurationAt(DateTimeOffset now) => (EndedAt ?? now) - StartedAt;

    /// <summary>Returns a copy of this session that ended at the given time.</summary>
    /// <param name="endedAt">End time; values before the start are clamped to the start.</param>
    /// <returns>The ended session.</returns>
    public ProcessSession End(DateTimeOffset endedAt) =>
        this with { EndedAt = endedAt < StartedAt ? StartedAt : endedAt };
}
