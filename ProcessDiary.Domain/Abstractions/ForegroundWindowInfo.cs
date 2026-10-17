namespace ProcessDiary.Domain.Abstractions;

/// <summary>Snapshot of the current foreground window.</summary>
/// <param name="ProcessName">Name of the owning process.</param>
/// <param name="ExecutablePath">Full path of the owning executable.</param>
/// <param name="WindowTitle">Title of the window.</param>
/// <param name="ProcessId">Operating system process identifier.</param>
public sealed record ForegroundWindowInfo(
    string ProcessName,
    string ExecutablePath,
    string WindowTitle,
    int ProcessId);
