namespace ProcessDiary.Domain.Abstractions;

/// <summary>Reads information about the current foreground window.</summary>
public interface IForegroundWindowService
{
    /// <summary>Gets the current foreground window.</summary>
    /// <returns>The window information, or null when none can be determined.</returns>
    ForegroundWindowInfo? GetCurrent();
}
