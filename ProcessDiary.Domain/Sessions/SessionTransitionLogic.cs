using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Domain.Sessions;

/// <summary>Pure, side-effect free session transition rules.</summary>
public sealed class SessionTransitionLogic
{
    private readonly HashSet<string> _ignored;

    /// <summary>Initializes the rules.</summary>
    /// <param name="ownProcessName">Name of the ProcessDiary process, which is always ignored.</param>
    /// <param name="additionalIgnored">Further process names to ignore, besides <c>explorer.exe</c>.</param>
    public SessionTransitionLogic(string ownProcessName, IEnumerable<string>? additionalIgnored = null)
    {
        _ignored = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "explorer.exe",
            "explorer",
            Normalize(ownProcessName)
        };
        _ignored.Add(ownProcessName);
        if (additionalIgnored is not null)
        {
            foreach (var name in additionalIgnored)
            {
                _ignored.Add(name);
                _ignored.Add(Normalize(name));
            }
        }
    }

    /// <summary>Determines whether a process is ignored and must not start or end sessions.</summary>
    /// <param name="processName">Name of the process, with or without the <c>.exe</c> extension.</param>
    /// <returns>True when the process is empty or on the ignore list.</returns>
    public bool IsIgnored(string processName) =>
        string.IsNullOrWhiteSpace(processName) || _ignored.Contains(processName) || _ignored.Contains(Normalize(processName));

    /// <summary>Evaluates a foreground change against the current session.</summary>
    /// <param name="current">The currently open session, if any.</param>
    /// <param name="foreground">Current foreground window, or null if unknown.</param>
    /// <param name="now">The time of the change.</param>
    /// <param name="idFactory">Creates the identifier for a newly started session.</param>
    /// <returns>The ended and started sessions, or <see cref="SessionTransition.None"/>.</returns>
    public SessionTransition Evaluate(ProcessSession? current, ForegroundWindowInfo? foreground, DateTimeOffset now, Func<Guid> idFactory)
    {
        // Ignored or unknown windows never start or end a session; the previous session continues.
        if (foreground is null || IsIgnored(foreground.ProcessName))
        {
            return SessionTransition.None;
        }

        if (current is not null && string.Equals(current.ProcessName, foreground.ProcessName, StringComparison.OrdinalIgnoreCase))
        {
            return SessionTransition.None;
        }

        var ended = current?.End(now);
        var started = new ProcessSession(idFactory(), foreground.ProcessName, foreground.ExecutablePath, foreground.WindowTitle, now);
        return new SessionTransition(ended, started);
    }

    /// <summary>Ends the open session, for example on pause or shutdown.</summary>
    /// <param name="current">The currently open session, if any.</param>
    /// <param name="now">The end time.</param>
    /// <returns>The ended session, or null when there was none.</returns>
    public static ProcessSession? Close(ProcessSession? current, DateTimeOffset now) => current?.End(now);

    private static string Normalize(string name) =>
        name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? name[..^4] : name;
}
