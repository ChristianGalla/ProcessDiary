namespace ProcessDiary.Domain.Abstractions;

/// <summary>Keeps a window out of screenshots so recording indicators never appear in the diary.</summary>
public interface IWindowCaptureProtection
{
    /// <summary>Excludes a window from screen capture.</summary>
    /// <param name="windowHandle">Native handle of the window.</param>
    /// <returns>True when the window is excluded from capture.</returns>
    bool ExcludeFromCapture(nint windowHandle);
}
